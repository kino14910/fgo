using System.Reflection;
using Fgo.Scripts.Cards;
using Fgo.Scripts.Character;
using Fgo.Scripts.Relics;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.UI;
using Fgo.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.AutoSlay;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.RunData;
using STS2RitsuLib.RuntimeInput;

// using Fgo.Scripts.Utils;

namespace Fgo.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Fgo";
    public const string ResPath = $"res://{ModId}";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    private static IDisposable? _runStartedSubscription;
    private static IDisposable? _runLoadedSubscription;
    private static IDisposable? _gameReadySubscription;
    private static IRuntimeHotkeyHandle? _nobleDeckHotkey;

    /// <summary>
    ///     局内保存的 FGO 玩家状态（令咒数量等）。
    ///     令咒语义: 使用后仅更新内存值，战斗胜利时同步到 RunSavedData；
    ///     退出战斗中再继续会恢复到战前值，打赢后下一场战斗保留战后值。
    /// </summary>
    public static PlayerRunSavedData<FgoRunState> RunState { get; private set; } = null!;

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Intuition, RoadOfFallingFlowers>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<SaintQuartz, SummonTicket>();
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterModSettingsReflectionProvider<FgoReflectedSettings>();
        FgoEnums.Initialize(ModId);
        FgoCombatUi.Initialize();

        // 注册局内保存数据（令咒数量）
        using (RitsuLibFramework.BeginModDataRegistration(ModId))
        {
            var store = RitsuLibFramework.GetRunSavedDataStore(ModId);
            RunState = store.RegisterPerPlayer(
                "fgo_run_state",
                () => new FgoRunState(),
                new RunSavedDataOptions
                {
                    WritePolicy = RunSavedDataWritePolicy.WhenNonDefault
                });
        }

        // Noble 卡牌图书馆筛选按钮: 调用 RitsuLib 的注册 API，
        // 由 RitsuLib 的 CardLibraryCompendiumPatch 在 NCardLibrary._Ready
        // postfix 中自动构造按钮、加入 strip、连接信号、注册到 _poolFilters。
        // 详见 NoblePoolFilterRegistration 上的文档注释。
        ModContentRegistry
            .For(ModId)
            .RegisterCardLibraryCompendiumSharedPoolFilter<NobleCardPool>(
                NoblePoolFilterRegistration.StableId,
                NoblePoolFilterRegistration.IconTexturePath);

        // Noble 卡相关 patch。全部通过 Ritsu 的 patcher 模式注册
        // （IPatchMethod + CreatePatcher + ApplyRequiredPatcher），
        // 不直接用 new Harmony(...).PatchAll() —— 那样绕过了 Ritsu 的诊断/回滚机制。
        var noblePatcher = RitsuLibFramework.CreatePatcher(ModId, "noble_cards");
        noblePatcher.RegisterPatch<NobleCardHideBannerPatch>();
        noblePatcher.RegisterPatch<NobleRaritySortPatch>();
        noblePatcher.RegisterPatch<NobleTitleOutlinePatch>();
        noblePatcher.RegisterPatch<NobleRarityLockPatch>();
        noblePatcher.RegisterPatch<NoblePoolPredicatePatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            noblePatcher,
            DisableMod,
            "Noble card patcher failed; Noble pool UI features will not work.");

        // 订阅 run 开始/加载事件，从 RunSavedData 恢复令咒数量
        _runStartedSubscription = RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(OnRunStarted);
        _runLoadedSubscription = RitsuLibFramework.SubscribeLifecycle<RunLoadedEvent>(OnRunLoaded);

        FgoTelemetry.Register();

        // 游戏就绪后注册 N 快捷键打开 NobleDeck
        _gameReadySubscription = RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(OnGameReady);
        try
        {
            var harmony = new Harmony("Fgo.autoslay.fix");
            harmony.PatchAll(typeof(NGamePatch).Assembly);
            Logger.Info("[Fgo] Applied autoslay release patch.");
        }
        catch (Exception ex)
        {
            Logger.ErrorNoTrace($"[Fgo] Failed to apply autoslay patch: {ex}");
        }
    }

    private static void OnRunStarted(RunStartedEvent evt)
    {
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);
    }

    private static void OnRunLoaded(RunLoadedEvent evt)
    {
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);
    }

    /// <summary>
    ///     为所有 FGO 角色玩家从 RunSavedData 加载令咒数量到各自独立的 FgoPlayerState。
    /// </summary>
    private static void LoadCommandSpellForFgoPlayers(RunState runState)
    {
        foreach (var player in runState.Players.Where(p => p.Character is FgoCharacter))
            FgoBattleHooks.Get(player).LoadCommandSpellFromRunState(player);

        Logger.Info("[Fgo] Loaded command spell count from RunSavedData for FGO players");
    }

    /// <summary>
    ///     为所有 FGO 角色玩家播种 NobleDeck 初始宝具卡（幂等）。
    ///     NobleDeck 是 RunPersistent 牌堆，由 RitsuLib 按 Player 索引并随存档序列化；
    ///     多人同时选择 FGO 角色时，也必须各自正确播种。因此播种与遗物（SaintQuartz/SummonTicket）
    ///     解耦，改由 run 生命周期统一处理，避免依赖遗物 AfterObtained 的时序导致客机票堆为空。
    /// </summary>
    private static void InitializeNobleDecks(RunState runState)
    {
        foreach (var player in runState.Players.Where(p => p.Character is FgoCharacter))
            FgoCardActions.EnsureNobleDeckSeeded(player);
    }

    private static void OnGameReady(GameReadyEvent evt)
    {
        // 注册 N 快捷键打开 NobleDeck
        try
        {
            _nobleDeckHotkey = RuntimeHotkeyService.Register("N", OnNobleDeckHotkey,
                new RuntimeHotkeyOptions
                {
                    Id = "fgo_open_noble_deck",
                    DebugName = "FGO NobleDeck viewer"
                });
            Logger.Info("[Fgo] Registered N hotkey for NobleDeck viewer");
        }
        catch (Exception ex)
        {
            Logger.ErrorNoTrace($"[Fgo] Failed to register N hotkey: {ex}");
        }
    }

    /// <summary>
    ///     N 快捷键回调: 找到顶栏 NobleDeck 按钮并触发打开。
    ///     若按钮不存在（非 FGO 角色）或未绑定 player，则什么都不做。
    /// </summary>
    private static void OnNobleDeckHotkey()
    {
        var globalUi = NRun.Instance?.GlobalUi;
        if (globalUi == null) return;
        var button = globalUi.FindChild("ModCardPileButton_FGO_CARDPILE_NOBLE", true, false) as NModCardPileButton;
        button?.TriggerOpen();
    }

    private static void DisableMod()
    {
        // 关键 patch 失败时调用。目前没有需要全局禁用的状态，
        // 但 Ritsu 要求传入一个 disable 回调以满足契约。
        Logger.ErrorNoTrace("[Fgo] Critical patch failed, mod disabled.");
    }
}

/// <summary>
///     FGO 局内保存数据: 随存档序列化，退出再继续会恢复。
/// </summary>
public sealed class FgoRunState
{
    /// <summary>
    ///     令咒数量（0-3）。默认 3。
    /// </summary>
    public int CommandSpellCount { get; set; } = 3;

    /// <summary>
    ///     圣晶石/召唤券共享计数（每层 +1，≥3 可右键获取宝具卡）。
    ///     存于按玩家数据而非遗物实例: 点金石精炼（SaintQuartz → SummonTicket）会替换遗物实例，
    ///     挂在实例上的状态会丢失，而按玩家数据不受影响。
    /// </summary>
    public int QuartzCount { get; set; }
}

[HarmonyPatch(typeof(NGame), nameof(NGame.IsReleaseGame))]
public static class NGamePatch
{
    public static void Postfix(ref bool __result)
    {
        __result = false;
    }
}

/// <summary>
///     AutoSlay 选角拦截: AutoSlayer.PlayMainMenuAsync 调用 _random.NextItem(items).Select()
///     随机选角。之前尝试 patch 泛型方法 Rng.NextItem&lt;T&gt; 的定义、以及对 async 方法
///     PlayMainMenuAsync 做 transpiler，均不可靠（泛型 patch 不应用到特化版本；
///     async 方法编译为状态机，transpiler 实际操作 MoveNext，IL 匹配困难）。
///     这里改为 Prefix patch NCharacterSelectButton.Select(): AutoSlay 激活时若被选中的
///     不是 FGO 按钮，重定向到同级的 FGO 按钮调用其 Select()。不依赖泛型 patch 与 IL 匹配。
/// </summary>
[HarmonyPatch(typeof(NCharacterSelectButton), nameof(NCharacterSelectButton.Select))]
public static class FgoCharacterSelectPatch
{
    private static bool Prefix(NCharacterSelectButton __instance)
    {
        if (!AutoSlayer.IsActive)
            return true; // 非自动测试，照常

        // 已经是 FGO 按钮，放行（重定向时调用 fgoBtn.Select() 会再次进入此处并放行，不会递归）
        if (__instance.Character is FgoCharacter)
            return true;

        // AutoSlay 随机选到了非 FGO 按钮，重定向到 FGO 按钮
        var fgoBtn = FindFgoButton(__instance);
        if (fgoBtn != null && fgoBtn != __instance)
        {
            Entry.Logger.Info(
                $"[Fgo] Autoslay: redirect Select() from {__instance.Character.Id} to FGO {fgoBtn.Character.Id}");
            fgoBtn.Select();
            return false; // 跳过原 Select
        }

        // 找不到 FGO 按钮（mod 未加载/角色未解锁），放行避免卡死选角流程
        Entry.Logger.Warn(
            $"[Fgo] Autoslay: FGO button not found, fallback to {__instance.Character.Id}");
        return true;
    }

    private static NCharacterSelectButton? FindFgoButton(NCharacterSelectButton from)
    {
        // AutoSlayer 选角时所有按钮已在同一 ButtonContainer 下并已 Init（Character 已赋值）。
        // 遍历父节点的子节点找 FGO 角色按钮。
        var parent = from.GetParent();
        if (parent == null) return null;

        foreach (var child in parent.GetChildren())
            if (child is NCharacterSelectButton btn
                && btn.Character is FgoCharacter
                && !btn.IsLocked)
                return btn;

        return null;
    }
}