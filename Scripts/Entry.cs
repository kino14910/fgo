using System.Reflection;
using Fgo.Scripts.Cards;
using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Patches;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Relics;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.UI;
using Fgo.Scripts.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Networking.ManagedActions;
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
    private static IDisposable? _mainMenuReadySubscription;
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
        FgoConfigSync.EnsureRegistered();
        FgoSkinSync.Init();
        FgoNobleDeckSync.Init();
        FgoQuartzSync.Init();
        FgoEnums.Initialize(ModId);
        FgoCombatUi.Initialize();

        // 注册 UI 按钮（NP 条/令咒）的托管网络动作 descriptor。
        // 必须在任何 peer 可能发起请求前于所有端注册完成，
        // 否则 ExecuteAction 按 opcode 找不到注册会抛异常。
        // 卡牌效果（如 SecondLife 即死爪牙）走 OnPlay，出牌本身已复制，不需要托管动作。
        RitsuLibManagedNetActions.Register(FgoNoblePhantasmCmd.SyncDescriptor);
        RitsuLibManagedNetActions.Register(FgoCommandSpellCmd.SyncDescriptor);

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

        // FTUE 首见标记。必须单开一个 using 块：上面那个用的是 GetRunSavedDataStore，
        // 这里用的是 GetDataStore（两个不同的 store 实例）。
        FgoFtue.Register();

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

        // 选人页设置面板 patch（修改 FgoReflectedSettings 的滑块/开关）。
        var charSelectPatcher = RitsuLibFramework.CreatePatcher(ModId, "character_select_settings");
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsReadyPatch>();
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsSelectPatch>();
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsClosedPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            charSelectPatcher,
            DisableMod,
            "Character select settings patcher failed; in-run settings panel will not work.");

        // 领袖气质（Charisma）: 能量不足时仍可打出（缺失能量改由生命值支付）。
        var cardCostPatcher = RitsuLibFramework.CreatePatcher(ModId, "card_cost");
        cardCostPatcher.RegisterPatch<FgoCharismaCostPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            cardCostPatcher,
            DisableMod,
            "Charisma cost patcher failed; Charisma cannot be played with insufficient energy.");

        // 原版手牌输入护栏: 〔虚数空间〕进出时原版手牌被整体搬空并隐藏，拖动出牌期间
        // NPlayerHand._draggedHolderIndex 与可见牌位数脱节，_UnhandledInput 里的
        // list.Insert 会抛 IndexOutOfRange（打〔虚数大海战〕时必现那条 ERROR）。
        // 索引越界时跳过本次输入即可，无其他副作用。
        var handInputPatcher = RitsuLibFramework.CreatePatcher(ModId, "player_hand_input");
        handInputPatcher.RegisterPatch<FgoPlayerHandInputPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            handInputPatcher,
            DisableMod,
            "Player hand input guard patcher failed; card shortcut keys may crash during void space transitions.");

        // 角色皮肤（战斗生物视觉 + 商家房间）。这两个原本是 [HarmonyPatch] 属性式 patch，
        // 靠旧的 Harmony.PatchAll(typeof(NGamePatch).Assembly) 注册；0.113 适配移除 AutoSlay patch 时
        // 该入口一并删除，而 vanilla 不 PatchAll(Fgo.dll)、Ritsu 也不扫 [HarmonyPatch] 特性，
        // 导致皮肤不再套用且 VisualsCache 为空（连带 ReapplySkin 失效）。现统一改走 IPatchMethod 模式。
        var skinPatcher = RitsuLibFramework.CreatePatcher(ModId, "skin_visuals");
        skinPatcher.RegisterPatch<FgoCreatureSkinPatch>();
        skinPatcher.RegisterPatch<FgoMerchantSkinPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            skinPatcher,
            DisableMod,
            "Skin visual patcher failed; character skins will not apply in combat or merchant rooms.");

        // 领袖气质（Charisma）紫光: 只能靠该能力超能量额打出的卡，手牌描边改为紫色。
        // 原版 NCardHighlight 只有青(playable)/金/红三色，任意色必须走 RitsuLib 的
        // ModCardHandOutlineRegistry —— 它 postfix NHandCardHolder.UpdateCard 把
        // CardHighlight.Modulate 写成规则颜色；refreshEveryFrame 的规则每帧重算，
        // 打出卡后能量变化自动刷新，无需手动 refresh。
        // 注册在 CardModel 上 ⇒ 覆盖所有卡（FGO 卡/无色/原版卡），与能力的全局语义一致；
        // 判定逻辑收敛在 CharismaPower.ShouldGlowPurple（持有能力 + 出牌阶段 + 可打出 + 能量缺口）。
        // 须在 ModelDb 冻结内容注册前调用（与上面的 RegisterCardLibraryCompendiumSharedPoolFilter 同）。
        ModContentRegistry
            .For(ModId)
            .RegisterCardHandOutline<CardModel>(card =>
                CharismaPower.ShouldGlowPurple(card) ? CharismaPower.GlowColor : null);

        // 订阅 run 开始/加载事件，从 RunSavedData 恢复令咒数量
        _runStartedSubscription = RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(OnRunStarted);
        _runLoadedSubscription = RitsuLibFramework.SubscribeLifecycle<RunLoadedEvent>(OnRunLoaded);

        FgoTelemetry.Register();

        // 游戏就绪后注册 N 快捷键打开 NobleDeck
        _gameReadySubscription = RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(OnGameReady);

        // 玩家在主菜单设置页改了设置后，回到主菜单时同步一次。
        _mainMenuReadySubscription = RitsuLibFramework.SubscribeLifecycle<MainMenuReadyEvent>(OnMainMenuReady);
    }

    private static void OnMainMenuReady(MainMenuReadyEvent evt)
    {
        FgoReflectedSettings.ReflectBoundValues();
        RegisterNobleDeckHotkey();
    }

    private static void OnRunStarted(RunStartedEvent evt)
    {
        FgoConfigSync.SyncAtRunStart(RunManager.Instance);
        // 每个端在此把自己的皮肤作为权威值再推一次：握手事件常早于 NetService 就绪（会早退），
        // 开局是"必然已有 NetService"的确定时点，可补上那次漏发。
        FgoSkinSync.OnSessionBound();
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);

        // 新局边界刷新 NobleDeck 同步的 Epoch 并重播一次，避免跨重启/跨局残留的旧版本基数挡住新快照。
        FgoNobleDeckSync.OnRunStarted();
    }

    private static void OnRunLoaded(RunLoadedEvent evt)
    {
        // 读档同样要同步配置："重启游戏 → 继续存档" 不会再触发 RunStartedEvent，
        // 若此处不同步，NetworkBaseNpPerCost 会停在 CLR 默认值（0），导致打出卡牌按费用获取宝具值失效。
        FgoConfigSync.SyncAtRunStart(RunManager.Instance);
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);

        FgoNobleDeckSync.OnRunStarted();

        // 读档回来时同样补发一次皮肤：期间可能有队友的皮肤从未同步到（例如对方中途加入）。
        FgoSkinSync.OnSessionBound();

        // 读档回到当前房间时：客户端置「跳过下一次房间进入自增」标志抵消重放多 +1，
        // 主机把全员圣晶石计数广播一次兜底，保证读档后客户端与主机一致。
        FgoQuartzSync.OnRunLoaded(evt.RunState);
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
        // 主菜单就绪、设置页已预注册后，把已持久化的反射设置回填到静态属性，
        // 确保 NobleCardModel.CanonicalEnergyCost 等依赖开关值的覆盖在规范卡首次访问费用前读到正确值。
        FgoReflectedSettings.ReflectBoundValues();

        // 注册打开 NobleDeck 的快捷键：从设置页读取已持久化（SaveScope.Global）的按键绑定，
        // 由 RitsuLib 的 RuntimeHotkeyService 在共享输入节点上路由回调。
        RegisterNobleDeckHotkey();
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

    /// <summary>
    ///     从设置页读取 NobleDeck 快捷键绑定并注册到 RitsuLib 运行时热键路由器。
    ///     绑定串为空（用户在设置中清空）时跳过注册；重复调用会先释放旧句柄避免重复注册。
    /// </summary>
    private static void RegisterNobleDeckHotkey()
    {
        _nobleDeckHotkey?.Dispose();
        _nobleDeckHotkey = null;

        var binding = FgoReflectedSettings.OpenNobleDeckKeyBinding;
        if (string.IsNullOrWhiteSpace(binding))
        {
            Logger.Warn("[Fgo] NobleDeck hotkey binding is empty; skipping registration.");
            return;
        }

        try
        {
            _nobleDeckHotkey = RuntimeHotkeyService.Register(
                binding,
                OnNobleDeckHotkey,
                new RuntimeHotkeyOptions
                {
                    Id = "fgo_open_noble_deck",
                    DebugName = "FGO NobleDeck viewer"
                });
            Logger.Info($"[Fgo] Registered NobleDeck hotkey '{binding}'.");
        }
        catch (Exception ex)
        {
            Logger.ErrorNoTrace($"[Fgo] Failed to register NobleDeck hotkey '{binding}': {ex}");
        }
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
    public int CommandSpellCount { get; set; } = 3;

    /// <summary>
    ///     圣晶石/召唤券共享计数（每层 +1，≥3 可右键获取宝具卡）。
    ///     存于按玩家数据而非遗物实例: 点金石精炼（SaintQuartz → SummonTicket）会替换遗物实例，
    ///     挂在实例上的状态会丢失，而按玩家数据不受影响。
    /// </summary>
    public int QuartzCount { get; set; }

    /// <summary>
    ///     该玩家选择的皮肤（<see cref="CharacterSkinId" /> 的整数值）。
    ///     按玩家存储并随存档/联机自动同步：选人页的「人物皮肤」是 per-machine 设置，
    ///     联机时各端值不同会导致所有玩家都显示为本机皮肤；改为按玩家记录后，
    ///     每个玩家的生物视觉应用"自己选择的皮肤"，跨端一致。
    /// </summary>
    public int Skin { get; set; }

    /// <summary>
    ///     存在这里（而非某张卡实例）的原因: 加成由"另一张卡"赋予，且同一位玩家只会有一份。
    /// </summary>
    public int GreatVoidSeaBattleBonusNp { get; set; }

    /// <summary>
    ///     由 <c>GreatVoidSeaBattle.EnsureLikeABird</c> 按本标志补齐（幂等，随存档保存）。
    /// </summary>
    public bool LikeABirdUnlocked { get; set; }
}