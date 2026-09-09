using System.Text.Json;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;

namespace Fgo.Scripts;

/// <summary>
///     将「0 费宝具」「每点能量回 NP」两个 per-machine 设置同步为联机局内运行时值。
///     联机时由主机在开局广播自身设置，所有端统一读取 Network* 值；单机回退到本机设置。
///     CanonicalEnergyCost 会被 CardEnergyCost 缓存，因此同步必须在卡牌创建前完成——
///     主机在 RunStartedEvent 广播，确保首场战斗前各端已拿到一致值。
/// </summary>
public static class FgoConfigSync
{
    public const string Topic = "fgo_config";

    public sealed record FgoConfig(bool NoCostNoblePhantasm, int BaseNpPerCost);

    public static bool NetworkNoCostNoblePhantasm { get; private set; }
    public static int NetworkBaseNpPerCost { get; private set; }

    private static bool _subscribed;

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
    }

    /// <summary>
    ///     每局开始时调用：本机先把设置回填到运行时值（单机直接用，联机作为快照到达前的回退），
    ///     主机再重新注册并以自身最新设置广播，使各端在卡牌创建前收敛到同一值。
    /// </summary>
    public static void SyncAtRunStart(RunManager? runManager)
    {
        var config = CurrentConfig();
        NetworkNoCostNoblePhantasm = config.NoCostNoblePhantasm;
        NetworkBaseNpPerCost = config.BaseNpPerCost;

        if (runManager?.NetService is NetHostGameService host)
        {
            RitsuLibSidecarConfigSyncService.RegisterTopic<FgoConfig, FgoConfig>(
                Topic,
                config,
                (_, _) => false,
                (_, delta) => delta);
            RitsuLibSidecarConfigSyncService.PublishHostState(host, Topic, 0, "run_start");
        }
    }

    private static FgoConfig CurrentConfig() =>
        new(FgoReflectedSettings.EnableNoCostNoblePhantasm, FgoReflectedSettings.BaseNpPerCost);

    private static void OnTopicChanged(SidecarConfigTopicChangedEvent e)
    {
        if (e.Topic != Topic) return;
        var cfg = JsonSerializer.Deserialize<FgoConfig>(e.StateJson);
        if (cfg is null) return;
        NetworkNoCostNoblePhantasm = cfg.NoCostNoblePhantasm;
        NetworkBaseNpPerCost = cfg.BaseNpPerCost;
    }
}
