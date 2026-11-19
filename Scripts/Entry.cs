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
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles.Nodes;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interactions.RightClick;
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

        // 必须在任何 peer 发起请求前于所有端注册，否则 ExecuteAction 按 opcode 找不到注册会抛异常。
        RitsuLibManagedNetActions.Register(FgoNoblePhantasmCmd.SyncDescriptor);
        RitsuLibManagedNetActions.Register(FgoCommandSpellCmd.SyncDescriptor);
        RitsuLibManagedNetActions.Register(FgoMoonRabbitFormCmd.SyncDescriptor);
        ModRightClickRegistry.Register(new FgoMoonRabbitRightClickHandler());

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

        // 必须单开 using 块：上面用 GetRunSavedDataStore、这里用 GetDataStore，是不同 store 实例。
        FgoFtue.Register();

        ModContentRegistry
            .For(ModId)
            .RegisterCardLibraryCompendiumSharedPoolFilter<NobleCardPool>(
                NoblePoolFilterRegistration.StableId,
                NoblePoolFilterRegistration.IconTexturePath);

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

        var charSelectPatcher = RitsuLibFramework.CreatePatcher(ModId, "character_select_settings");
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsReadyPatch>();
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsSelectPatch>();
        charSelectPatcher.RegisterPatch<FgoCharacterSelectSettingsClosedPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            charSelectPatcher,
            DisableMod,
            "Character select settings patcher failed; in-run settings panel will not work.");

        var cardCostPatcher = RitsuLibFramework.CreatePatcher(ModId, "card_cost");
        cardCostPatcher.RegisterPatch<FgoCharismaCostPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            cardCostPatcher,
            DisableMod,
            "Charisma cost patcher failed; Charisma cannot be played with insufficient energy.");

        // 虚数空间进出时原版手牌被搬空隐藏，拖动期间 list.Insert 索引会越界抛异常；护栏跳过该次输入即可。
        var handInputPatcher = RitsuLibFramework.CreatePatcher(ModId, "player_hand_input");
        handInputPatcher.RegisterPatch<FgoPlayerHandInputPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            handInputPatcher,
            DisableMod,
            "Player hand input guard patcher failed; card shortcut keys may crash during void space transitions.");

        var skinPatcher = RitsuLibFramework.CreatePatcher(ModId, "skin_visuals");
        skinPatcher.RegisterPatch<FgoCreatureSkinPatch>();
        skinPatcher.RegisterPatch<FgoMerchantSkinPatch>();
        RitsuLibFramework.ApplyRequiredPatcher(
            skinPatcher,
            DisableMod,
            "Skin visual patcher failed; character skins will not apply in combat or merchant rooms.");

        ModContentRegistry
            .For(ModId)
            // 任意色描边必须走 RitsuLib 的 ModCardHandOutlineRegistry（原版只有青/金/红）；注册在 CardModel 上覆盖所有卡。
            .RegisterCardHandOutline<CardModel>(card =>
                CharismaPower.ShouldGlowPurple(card) ? CharismaPower.GlowColor : null);

        _runStartedSubscription = RitsuLibFramework.SubscribeLifecycle<RunStartedEvent>(OnRunStarted);
        _runLoadedSubscription = RitsuLibFramework.SubscribeLifecycle<RunLoadedEvent>(OnRunLoaded);

        FgoTelemetry.Register();

        _gameReadySubscription = RitsuLibFramework.SubscribeLifecycle<GameReadyEvent>(OnGameReady);

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
        FgoSkinSync.OnSessionBound();
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);

        FgoNobleDeckSync.OnRunStarted();
    }

    private static void OnRunLoaded(RunLoadedEvent evt)
    {
        FgoConfigSync.SyncAtRunStart(RunManager.Instance);
        LoadCommandSpellForFgoPlayers(evt.RunState);
        InitializeNobleDecks(evt.RunState);

        FgoNobleDeckSync.OnRunStarted();

        FgoSkinSync.OnSessionBound();

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
        FgoReflectedSettings.ReflectBoundValues();

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