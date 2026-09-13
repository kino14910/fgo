using System;
using System.Text.Json;
using Fgo.Scripts.Character;
using Fgo.Scripts.Relics;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;
using STS2RitsuLib.RunData;

namespace Fgo.Scripts;

/// <summary>
///     圣晶石计数联机同步（基于 RitsuLib Sidecar 消息：主机权威广播 + 客户端读档重放跳过）。
///     AfterRoomEntered 是被复制的房间动作：正常推进时所有端都会重放，各端本地 +1 即天然一致，
///     无需任何门控（与原版 QuartzCounter++ 行为一致，单机/正常联机都正确）。
///     唯一的真分歧在读档：主机以存档为准、不重放 AfterRoomEntered；客机重建当前房间会重放 → 多 +1。
///     修复：RunLoadedEvent 时仅客户端置「跳过下一次房间进入」标志，消耗于读档后的首次 AfterRoomEntered，
///     使客机不再对「已在存档里计过」的当前房间重复 +1；主机则在此时把全员计数广播一次兜底。
/// </summary>
public static class FgoQuartzSync
{
    public record QuartzSyncMessage(ulong NetId, int QuartzCount);

    private static readonly RitsuLibSidecarMessageDescriptor<QuartzSyncMessage> QuartzSyncDescriptor = new(
        ModuleId: Entry.ModId,
        MessageKey: "fgo_quartz_sync_v1",
        Serialize: static msg => JsonSerializer.SerializeToUtf8Bytes(msg),
        Deserialize: static bytes => JsonSerializer.Deserialize<QuartzSyncMessage>(bytes)!,
        Delivery: RitsuLibSidecarDeliverySemantics.StableSync);

    private static IDisposable? _subscription;
    private static bool _handshakeSubscribed;
    private static bool _skipNextRoomEntryOnClient;

    public static void Init()
    {
        _subscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(QuartzSyncDescriptor, OnReceived);
        if (!_handshakeSubscribed)
        {
            _handshakeSubscribed = true;
            RitsuLibSidecarEvents.OnHandshakeCompleted(_ => BroadcastAll(RunManager.Instance?.DebugOnlyGetState()));
        }
    }

    /// <summary>
    ///     读档后由 Entry.OnRunLoaded 调用：客户端标记「跳过下一次房间进入自增」（抵消读档重放当前房间），
    ///     主机/单机则把权威计数广播一次兜底（单机无对端，仅本地缓存、无害）。
    /// </summary>
    public static void OnRunLoaded(RunState? runState)
    {
        if (RunManager.Instance?.NetService is NetClientGameService)
            _skipNextRoomEntryOnClient = true;
        else
            BroadcastAll(runState);
    }

    /// <summary>
    ///     读档重放当前房间时（仅客户端、且仅首次）返回 true 并消费标志，使该次 AfterRoomEntered 不重复 +1。
    ///     正常推进 / 单机 / 主机恒返回 false —— 各端照常本地自增。
    /// </summary>
    public static bool ShouldSkipRoomEntry()
    {
        if (!_skipNextRoomEntryOnClient) return false;
        _skipNextRoomEntryOnClient = false;
        return true;
    }

    /// <summary>
    ///     主机在读档/握手完成时把全员圣晶石计数广播一次，保证客户端（含后加入/重连）与主机一致。
    ///     运行期各端靠被复制的 AfterRoomEntered 本地自增收敛，无需逐次广播。
    /// </summary>
    public static void BroadcastAll(RunState? runState)
    {
        if (runState == null) return;
        var netService = RunManager.Instance?.NetService;
        if (netService is not NetHostGameService) return;

        foreach (var player in runState.Players.Where(p => p.Character is FgoCharacter))
        {
            var count = Entry.RunState.Get(player).QuartzCount;
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                netService, QuartzSyncDescriptor, new QuartzSyncMessage(player.NetId, count));
        }
    }

    /// <summary>
    ///     本地发生【不会被复制的】圣晶石计数变化后调用（典型：右键抽取消耗）。
    ///     increment（AfterRoomEntered）是被复制的房间动作，各端本地自增即一致，无需广播；
    ///     但右键消耗只在拥有者本机运行（其余端提前 return），而 QuartzCount 存于主机权威的
    ///     PlayerRunSavedData（客户端写不回传主机）——若不上报，主机侧该玩家计数永远不减，
    ///     且下次主机 BroadcastAll 会把客户端已扣的值覆盖回旧值。故由拥有者把最新计数上报/广播：
    ///     客户端发往主机，主机广播全员（并在收到客户端的值时转发给其它队友）。
    /// </summary>
    public static void NotifyLocalCount(Player? player)
    {
        try
        {
            if (player == null) return;
            var netService = RunManager.Instance?.NetService;
            if (netService == null) return;

            var count = Entry.RunState.Get(player).QuartzCount;
            var msg = new QuartzSyncMessage(player.NetId, count);
            switch (netService.Type)
            {
                case NetGameType.Client:
                    RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, QuartzSyncDescriptor, msg);
                    break;
                default:
                    RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, QuartzSyncDescriptor, msg);
                    break;
            }
        }
        catch (Exception ex)
        {
            // 同步是「尽力而为」的旁路：失败不得波及调用方的核心玩法（如右键扣费）。
            Entry.Logger.Warn($"[Fgo] Quartz count sync send failed: {ex}");
        }
    }

    private static void OnReceived(RitsuLibSidecarTypedDispatchContext<QuartzSyncMessage> context)
    {
        // 主机收到客户端上报后转发广播给其它队友（广播不回传发送方，无环）。
        if (context.IsHostIngest)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                RunManager.Instance?.NetService, QuartzSyncDescriptor, context.Message);

        // 计数写入与遗物高亮都触碰游戏状态，调度回 Godot 主线程，避开网络接收线程竞态。
        var msg = context.Message;
        Callable.From(() => ApplyCount(msg)).CallDeferred();
    }

    private static void ApplyCount(QuartzSyncMessage msg)
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;
        Entry.RunState.Modify(runState, msg.NetId, d => d.QuartzCount = msg.QuartzCount);
        RefreshVisual(msg.NetId);
    }

    private static void RefreshVisual(ulong netId)
    {
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;
        var player = runState.Players.FirstOrDefault(p => p.NetId == netId);
        if (player == null) return;

        player.GetRelic<SaintQuartz>()?.RefreshQuartzActivationVisual(SaintQuartz.CostPerChoice);
        player.GetRelic<SummonTicket>()?.RefreshQuartzActivationVisual(SummonTicket.CostPerChoice);
    }
}
