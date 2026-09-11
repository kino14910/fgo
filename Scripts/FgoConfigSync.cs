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

    public static bool NetworkNoCostNoblePhantasm { get; private set; }
    public static int NetworkBaseNpPerCost { get; private set; }

    public static void EnsureRegistered()
    {
        RitsuLibSidecarConfigSyncService.RegisterTopic<FgoConfig, FgoConfig>(
            Topic,
            CurrentConfig(),
            (_, _) => false,
            (_, delta) => delta);

        if (_subscribed) return;
        _subscribed = true;
        RitsuLibSidecarEvents.OnConfigTopicChanged(OnTopicChanged);
        RitsuLibSidecarEvents.OnHandshakeCompleted(OnHandshakeCompleted);
    }

    /// <summary>
    ///     会话握手完成（主机/客户端连上即触发，远早于任意一局的 RunStartedEvent）。
    ///     仅主机广播自身设置，使客户端在播种 NobleDeck / 创建卡牌前就拿到一致的规范费用，
    ///     避免各端 CanonicalEnergyCost 因读取本机设置而分歧。
    /// </summary>
    private static void OnHandshakeCompleted(SidecarHandshakeCompletedEvent _)
    {
        if (RunManager.Instance?.NetService is NetHostGameService host)
            RitsuLibSidecarConfigSyncService.PublishHostState(host, Topic, 0, "handshake");
    }

    /// <summary>
    ///     每局开始时调用。
    ///     - 主机：以自身设置作为权威值回填，并冗余广播一次（覆盖"握手后、开局前主机改了设置"的场景）。
    ///     - 客户端：保留握手阶段由主机快照写入的同步值，**切勿**用本机设置覆盖（否则已播种卡牌费用被缓存为本机值，跨端校验和分歧）。
    ///     - 单机：直接使用本机设置。
    /// </summary>
    public static void SyncAtRunStart(RunManager? runManager)
    {
        if (runManager?.NetService is NetHostGameService host)
        {
            var config = CurrentConfig();
            NetworkNoCostNoblePhantasm = config.NoCostNoblePhantasm;
            NetworkBaseNpPerCost = config.BaseNpPerCost;
            RitsuLibSidecarConfigSyncService.PublishHostState(host, Topic, 0, "run_start");
        }
        else if (runManager?.NetService is not NetClientGameService)
        {
            var config = CurrentConfig();
            NetworkNoCostNoblePhantasm = config.NoCostNoblePhantasm;
            NetworkBaseNpPerCost = config.BaseNpPerCost;
        }
    }

    private static FgoConfig CurrentConfig()
    {
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
        NetworkBaseNpPerCost = cfg.BaseNpPerCost;
    }

    public sealed record FgoConfig(bool NoCostNoblePhantasm, int BaseNpPerCost);
}