using System.Text.Json;
using Fgo.Scripts.Patches;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;
using STS2RitsuLib.RunData;

namespace Fgo.Scripts;

/// <summary>
///     联机皮肤同步（基于 RitsuLib Sidecar 消息，而非 PlayerRunSavedData）。
///     <see cref="PlayerRunSavedData{T}" /> 是主机权威通道：客户端对自己槽位的 Modify 不会回传主机，
///     于是主机端"远端玩家"皮肤永远缺值、客户端读到默认皮肤 —— 这正是「主机两台都显示主机皮肤、
///     客机显示默认皮肤」的根因。
///     这里改为 Sidecar 即时消息：客户端把自己的皮肤发给主机，主机收到后转发广播给所有队友，
///     每个端按玩家的 NetId 缓存到 <see cref="RemoteSkins" />，渲染时按拥有者套用，跨端一致。
/// </summary>
/// <remarks>
///     <para>
///         同步的可靠性要求来自一个具体故障：<b>第一次下载本 mod、从未在设置里动过皮肤选项的人</b>，
///         别人看他时会显示成默认形象。原因是皮肤缓存只靠"发出/收到消息"填充，而
///         <see cref="RitsuLibSidecarEvents.OnHandshakeCompleted" /> 触发时
///         <c>RunManager.Instance.NetService</c> 往往还没挂上（大厅加入阶段），于是
///         <see cref="SendSkinSync" /> 直接早退、从未发出，对端 <see cref="RemoteSkins" /> 里没有该玩家的键，
///         <see cref="ResolveSkin" /> 便回退到<b>本机</b>皮肤 —— 一台没同步到就整体显示本机（默认）外观。
///     </para>
///     <para>
///         因此这里做三件事：
///         ① 每个端在握手/开局时都把自己的皮肤作为权威值主动推一次（不依赖"只在改动时发"）；
///         ② 客户端可向主机请求全量重播，弥补中途加入 / 漏收；
///         ③ <see cref="ResolveSkin" /> 只在"本机玩家"身上回退到本机设置，远端玩家缺值时用
///         各端一致的确定默认值，绝不再拿本机皮肤去污染别人。
///     </para>
/// </remarks>
public static class FgoSkinSync
{
    /// <summary>
    ///     NetId -> 该玩家选择的皮肤（<see cref="FgoReflectedSettings.CharacterSkin" /> 的整数值）。
    ///     含本机玩家自身（SendSkinSync 时写入自身 NetId），因此单机 / 联机中的自己同样走这条缓存，
    ///     不再依赖 TryCreateCreatureVisuals 的兜底值，避免单机皮肤消失的回归。
    /// </summary>
    public static readonly Dictionary<ulong, int> RemoteSkins = new();

    /// <summary>
    ///     远端玩家皮肤未知时的确定默认值（迦勒底）。取"全新安装、从未改过设置"时会有的值，
    ///     因此各端对同一个未知玩家的显示一致，不会出现"我看他是我的皮肤、你看他是你的皮肤"。
    /// </summary>
    public const int NeutralDefaultSkin = (int)CharacterSkinId.Chaldea;

    private static readonly RitsuLibSidecarMessageDescriptor<SkinSyncMessage> SkinSyncDescriptor = new(
        Entry.ModId,
        "fgo_skin_sync_v1",
        static msg => JsonSerializer.SerializeToUtf8Bytes(msg),
        static bytes => JsonSerializer.Deserialize<SkinSyncMessage>(bytes)!,
        RitsuLibSidecarDeliverySemantics.StableSync);

    private static readonly RitsuLibSidecarMessageDescriptor<SkinResyncRequest> SkinResyncDescriptor = new(
        Entry.ModId,
        "fgo_skin_resync_v1",
        static _ => Array.Empty<byte>(),
        static _ => new SkinResyncRequest(),
        RitsuLibSidecarDeliverySemantics.StableSync);

    private static IDisposable? _subscription;
    private static IDisposable? _resyncSubscription;
    private static bool _handshakeSubscribed;

    // 渲染路径补发请求的节流（同一"未同步到"状态不重复刷请求）。
    private static readonly TimeSpan ResyncThrottle = TimeSpan.FromSeconds(10);
    private static DateTime _lastResyncRequestUtc = DateTime.MinValue;

    /// <summary>
    ///     在 Entry.Init 中调用一次：订阅皮肤消息 + 在会话握手完成时补发一次，
    ///     保证后加入的队友也能拿到全员皮肤。
    /// </summary>
    public static void Init()
    {
        _subscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(SkinSyncDescriptor, OnSkinSyncReceived);
        _resyncSubscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(SkinResyncDescriptor, OnResyncReceived);
        if (!_handshakeSubscribed)
        {
            _handshakeSubscribed = true;
            RitsuLibSidecarEvents.OnHandshakeCompleted(_ => OnSessionBound());
        }
    }

    /// <summary>
    ///     自己的皮肤同步出去。开局 / 换肤 / 读档时调用。
    /// </summary>
    public static void SendSkinSync()
    {
        FgoReflectedSettings.ReflectBoundValues();

        var netService = RunManager.Instance?.NetService;
        if (netService == null || netService.Type is NetGameType.None or NetGameType.Replay)
        {
            // 握手事件常先于 RunManager 挂上 NetService 触发（加入大厅阶段），此时直接返回会让
            // "从未改过设置的人"整局都不发皮肤。交由 RunStarted / 视觉创建时的自愈路径补发。
            Entry.Logger.Info("[Fgo] Skin sync skipped: network service not ready yet; will retry on run start.");
            return;
        }

        var skin = (int)FgoReflectedSettings.CharacterSkin;

        // 本机缓存自己的皮肤：单机 / 本机玩家渲染用，也是 postfix 的兜底来源
        RemoteSkins[netService.NetId] = skin;

        var msg = new SkinSyncMessage(netService.NetId, skin);
        switch (netService.Type)
        {
            // 客户端 → 主机；主机再转发广播给其它队友
            case NetGameType.Client:
                RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, SkinSyncDescriptor, msg);
                break;
            // 主机 / 单机 → 直接广播（单机无对端，仅本地缓存生效）
            default:
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, SkinSyncDescriptor, msg);
                break;
        }

        Entry.Logger.Info($"[Fgo] Sent skin sync: netId={netService.NetId}, skin={skin}, type={netService.Type}");
    }

    /// <summary>
    ///     解析某位玩家应显示的皮肤：优先取已同步的 <see cref="RemoteSkins" />。
    ///     <b>只有本机玩家</b>在未同步到时回退本机设置（单机、开局瞬间、本机自己的渲染）；
    ///     远端玩家缺值时返回各端一致的 <see cref="NeutralDefaultSkin" /> ——
    ///     绝不回退本机设置，否则"某个远端玩家没同步到"会表现为"他显示成我的皮肤"。
    /// </summary>
    public static int ResolveSkin(ulong netId)
    {
        if (RemoteSkins.TryGetValue(netId, out var skin))
            return skin;

        var localNetId = RunManager.Instance?.NetService?.NetId;
        if (localNetId == null || netId == localNetId.Value)
            return (int)FgoReflectedSettings.CharacterSkin;

        return NeutralDefaultSkin;
    }

    /// <summary>
    ///     某位远端玩家的皮肤是否已同步到（供渲染路径判断是否需要触发补发）。
    /// </summary>
    public static bool HasRemoteSkin(ulong netId)
    {
        return RemoteSkins.ContainsKey(netId);
    }

    /// <summary>
    ///     会话握手完成 / 每局开始时调用：每个端都把自己的皮肤作为权威值主动推一次，
    ///     客户端再请求主机全量重播（覆盖中途加入前主机已知的其它玩家皮肤）。
    ///     "只在改动时发"有致命缺口：从未改过设置的人永远不发，对端因此无值可读。
    /// </summary>
    public static void OnSessionBound()
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null || netService.Type is NetGameType.None or NetGameType.Replay)
            return;

        SendSkinSync();

        if (netService.Type == NetGameType.Client)
            RequestFullResync();
    }

    /// <summary>
    ///     客户端请求主机把全员皮肤整体重播一次，弥补漏收 / 中途加入。
    /// </summary>
    private static void RequestFullResync()
    {
        var netService = RunManager.Instance?.NetService;
        if (netService is not NetClientGameService) return;

        RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, SkinResyncDescriptor, new SkinResyncRequest());
        Entry.Logger.Info("[Fgo] Skin full resync requested from host.");
    }

    /// <summary>
    ///     渲染路径发现某位远端玩家皮肤缺值时调用（见 <c>FgoCreatureSkinPatch</c> / <c>FgoWorldSkin</c>）。
    ///     做节流：同一"未同步到"状态只请求一次，避免每场战斗 / 每个房间重复刷请求。
    ///     主机端无需求（主机是重播的发出方）。
    /// </summary>
    public static void RequestResyncFromVisualPath()
    {
        var netService = RunManager.Instance?.NetService;
        if (netService is not NetClientGameService) return;

        var now = DateTime.UtcNow;
        if (now - _lastResyncRequestUtc < ResyncThrottle) return;
        _lastResyncRequestUtc = now;

        RequestFullResync();
    }

    /// <summary>
    ///     主机收到客户端的全量重播请求：把当前已知的全员皮肤广播一次。
    /// </summary>
    private static void OnResyncReceived(RitsuLibSidecarTypedDispatchContext<SkinResyncRequest> context)
    {
        if (!context.IsHostIngest) return;

        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        foreach (var (netId, skin) in RemoteSkins)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                netService, SkinSyncDescriptor, new SkinSyncMessage(netId, skin));

        Entry.Logger.Info($"[Fgo] Skin full resync broadcasted to clients ({RemoteSkins.Count} entries).");
    }

    private static void OnSkinSyncReceived(RitsuLibSidecarTypedDispatchContext<SkinSyncMessage> context)
    {
        RemoteSkins[context.Message.NetId] = context.Message.Skin;

        // 主机收到客户端的皮肤后，转发广播给其它队友，保证全员一致。
        // 注意：不能照搬教程的 "Message.NetId != SenderNetId" 判定——客户端 SendToHost 时
        // 消息里的 NetId 就是发送者自身，二者相等会使该判定恒为假、转发永远不发生。
        // 这里只需判断 IsHostIngest（主机从客户端收到的消息），广播只发往对端、不会回传主机自身，无环。
        if (context.IsHostIngest)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                RunManager.Instance?.NetService, SkinSyncDescriptor, context.Message);

        // 若对应玩家的生物视觉已创建，立即按最新皮肤重套（覆盖视觉先于消息到达的情况）
        FgoCreatureSkinPatch.ReapplySkin(context.Message.NetId, context.Message.Skin);

        // 世界场景（火堆）形象同理：若该玩家的形象已建好，按最新皮肤刷新
        FgoWorldSkin.Refresh(context.Message.NetId, context.Message.Skin);

        Entry.Logger.Info($"[Fgo] Received skin sync: netId={context.Message.NetId}, skin={context.Message.Skin}");
    }

    public record SkinSyncMessage(ulong NetId, int Skin);

    public record SkinResyncRequest;
}
