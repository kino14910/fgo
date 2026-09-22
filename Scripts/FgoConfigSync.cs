using System.Text.Json;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;

namespace Fgo.Scripts;

/// <summary>
///     将「0 费宝具」「每点能量回 NP」两个 per-machine 设置同步为联机局内运行时值。
///     联机时由主机在会话握手完成时广播自身设置，所有端统一读取 Network* 值；单机回退到本机设置。
///     CanonicalEnergyCost 会在卡牌创建时被 CardEnergyCost 缓存（见 CardEnergyCost.Canonical），
///     因此主机配置必须在"客户端播种 NobleDeck"之前到达客户端——故在会话握手完成时即广播，
///     而非等到 RunStartedEvent（那时客户端早已创建完卡牌，晚到的快照无法修正已缓存的费用）。
/// </summary>
public static class FgoConfigSync
{
    public const string Topic = "fgo_config";

    private static bool _subscribed;

    /// <summary>是否已由「本机设置」或「远端快照」确定过权威值。</summary>
    private static bool _resolved;

    private static int _networkBaseNpPerCost;

    public static bool NetworkNoCostNoblePhantasm { get; private set; }

    /// <summary>
    ///     每点能量回复的 NP。**切勿在同步发生前把它当成 0**：C# 默认值是 0，而「同步尚未发生」与
    ///     「设置为 0」无法从原值区分——前者会让打出卡牌按费用获取 NP 恒为 0（表现为重启/读档后该机制失效）。
    ///     故未确定权威值且非联机对局时，回退读取本机设置；联机对局中权威值须来自主机快照，不能回退本机。
    /// </summary>
    public static int NetworkBaseNpPerCost =>
        _resolved || IsNetworkedRun() ? _networkBaseNpPerCost : LocalBaseNpPerCost;

    /// <summary>本机设置页里的 BaseNpPerCost（先回填磁盘持久化值再读取）。</summary>
    private static int LocalBaseNpPerCost
    {
        get
        {
            FgoReflectedSettings.ReflectBoundValues();
            return FgoReflectedSettings.BaseNpPerCost;
        }
    }

    public static void EnsureRegistered()
    {
        RegisterTopic(CurrentConfig());

        if (_subscribed) return;
        _subscribed = true;
        RitsuLibSidecarEvents.OnConfigTopicChanged(OnTopicChanged);
        RitsuLibSidecarEvents.OnHandshakeCompleted(OnHandshakeCompleted);
    }

    private static void RegisterTopic(FgoConfig initialState)
    {
        RitsuLibSidecarConfigSyncService.RegisterTopic<FgoConfig, FgoConfig>(
            Topic,
            initialState,
            (_, _) => false,
            (_, delta) => delta);
    }

    /// <summary>
    ///     会话握手完成（主机/客户端连上即触发，远早于任意一局的 RunStartedEvent）。
    ///     仅主机广播自身设置，使客户端在播种 NobleDeck / 创建卡牌前就拿到一致的规范费用，
    ///     避免各端 CanonicalEnergyCost 因读取本机设置而分歧。
    /// </summary>
    private static void OnHandshakeCompleted(SidecarHandshakeCompletedEvent _)
    {
        PublishHostConfig("handshake");
    }

    /// <summary>
    ///     每局开始时调用。
    ///     - 主机：以自身设置作为权威值回填，并冗余广播一次（覆盖"握手后、开局前主机改了设置"的场景）。
    ///     - 客户端：保留握手阶段由主机快照写入的同步值，**切勿**用本机设置覆盖（否则已播种卡牌费用被缓存为本机值，跨端校验和分歧）。
    ///     - 单机：直接使用本机设置。
    /// </summary>
    public static void SyncAtRunStart(RunManager? runManager)
    {
        if (runManager?.NetService is NetHostGameService)
        {
            PublishHostConfig("run_start");
        }
        else if (runManager?.NetService is not NetClientGameService)
        {
            var config = CurrentConfig();
            NetworkNoCostNoblePhantasm = config.NoCostNoblePhantasm;
            _networkBaseNpPerCost = config.BaseNpPerCost;
            _resolved = true;
        }
    }

    /// <summary>
    ///     主机广播当前设置。**关键**：<see cref="RitsuLibSidecarConfigSyncService.PublishHostState" />
    ///     只广播「主题缓存的状态」（即 <c>RegisterTopic</c> 时写入的 initial state），并不会去读
    ///     <see cref="CurrentConfig" />；而该 initial state 是 mod 初始化（<see cref="Entry.Init" />）时
    ///     用 C# 默认值写入的——彼时设置页尚未注册、<c>ReflectBoundValues</c> 还没把磁盘上的持久化值
    ///     回填到静态成员。若不修正，主机永远只广播默认值，客户端拿到的 BaseNpPerCost /
    ///     EnableNoCostNoblePhantasm 与主机不一致（联机设置不同步）。
    ///     因此：先用最新 <see cref="CurrentConfig" /> 重置主题状态，再广播。
    /// </summary>
    private static void PublishHostConfig(string reason)
    {
        if (RunManager.Instance?.NetService is not NetHostGameService host) return;

        var config = CurrentConfig();
        NetworkNoCostNoblePhantasm = config.NoCostNoblePhantasm;
        _networkBaseNpPerCost = config.BaseNpPerCost;
        _resolved = true;

        RegisterTopic(config);
        RitsuLibSidecarConfigSyncService.PublishHostState(host, Topic, 0, reason);

        Entry.Logger.Info(
            $"[Fgo] Host published config ({reason}): NoCostNoblePhantasm={config.NoCostNoblePhantasm}, BaseNpPerCost={config.BaseNpPerCost}");
    }

    private static FgoConfig CurrentConfig()
    {
        // 必须先回填：RitsuLib 的 [ModSettingsBinding] 静态镜像不会在启动时自动读磁盘，
        // 不调用 ReflectBoundValues() 的话这里读到的永远是 C# 默认值（BaseNpPerCost=5 / 关闭 0 费）。
        FgoReflectedSettings.ReflectBoundValues();
        return new FgoConfig(FgoReflectedSettings.EnableNoCostNoblePhantasm, FgoReflectedSettings.BaseNpPerCost);
    }

    /// <summary>
    ///     当前是否处于联网（多人）对局。本端 NetService 在主机为 <see cref="NetHostGameService" />、
    ///     客户端为 <see cref="NetClientGameService" />；单机为 <c>null</c> 或其它本地服务类型。
    /// </summary>
    public static bool IsNetworkedRun()
    {
        return RunManager.Instance?.NetService is NetHostGameService or NetClientGameService;
    }

    private static void OnTopicChanged(SidecarConfigTopicChangedEvent e)
    {
        if (e.Topic != Topic) return;
        var cfg = JsonSerializer.Deserialize<FgoConfig>(e.StateJson);
        if (cfg is null) return;
        NetworkNoCostNoblePhantasm = cfg.NoCostNoblePhantasm;
        _networkBaseNpPerCost = cfg.BaseNpPerCost;
        _resolved = true;

        Entry.Logger.Info(
            $"[Fgo] Applied config topic '{e.Topic}' ({e.Reason}): NoCostNoblePhantasm={cfg.NoCostNoblePhantasm}, BaseNpPerCost={cfg.BaseNpPerCost}");
    }

    public sealed record FgoConfig(bool NoCostNoblePhantasm, int BaseNpPerCost);
}