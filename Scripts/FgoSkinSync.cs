using System.Text.Json;
using Fgo.Scripts.Patches;
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
public static class FgoSkinSync
{
    /// <summary>
    ///     NetId -> 该玩家选择的皮肤（<see cref="FgoReflectedSettings.CharacterSkin" /> 的整数值）。
    ///     含本机玩家自身（SendSkinSync 时写入自身 NetId），因此单机 / 联机中的自己同样走这条缓存，
    ///     不再依赖 TryCreateCreatureVisuals 的兜底值，避免单机皮肤消失的回归。
    /// </summary>
    public static readonly Dictionary<ulong, int> RemoteSkins = new();

    private static readonly RitsuLibSidecarMessageDescriptor<SkinSyncMessage> SkinSyncDescriptor = new(
        Entry.ModId,
        "fgo_skin_sync_v1",
        static msg => JsonSerializer.SerializeToUtf8Bytes(msg),
        static bytes => JsonSerializer.Deserialize<SkinSyncMessage>(bytes)!,
        RitsuLibSidecarDeliverySemantics.StableSync);

    private static IDisposable? _subscription;
    private static bool _handshakeSubscribed;

    /// <summary>
    ///     在 Entry.Init 中调用一次：订阅皮肤消息 + 在会话握手完成时补发一次，
    ///     保证后加入的队友也能拿到全员皮肤。
    /// </summary>
    public static void Init()
    {
        _subscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(SkinSyncDescriptor, OnSkinSyncReceived);
        if (!_handshakeSubscribed)
        {
            _handshakeSubscribed = true;
            RitsuLibSidecarEvents.OnHandshakeCompleted(_ => SendSkinSync());
        }
    }

    /// <summary>
    ///     把自己的皮肤同步出去。开局 / 换肤 / 读档时调用。
    /// </summary>
    public static void SendSkinSync()
    {
        FgoReflectedSettings.ReflectBoundValues();

        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

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
    ///     解析某位玩家应显示的皮肤：优先取已同步的 <see cref="RemoteSkins" />；
    ///     尚未同步到（单机 / 开局瞬间 / 消息未达）时回退本机设置。
    /// </summary>
    public static int ResolveSkin(ulong netId)
    {
        return RemoteSkins.TryGetValue(netId, out var skin)
            ? skin
            : (int)FgoReflectedSettings.CharacterSkin;
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
}