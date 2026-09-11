using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.Sidecar;

namespace Fgo.Scripts;

/// <summary>
///     联机宝具卡组（NobleDeck）同步。
/// </summary>
/// <remarks>
///     NobleDeck 是 RunPersistent 的 mod 牌堆，其持久化走
///     <c>PlayerRunSavedData</c>（见 RitsuLib 的 ModCardPilePersistence：主机权威、
///     仅在存/读档时快照），运行期间本机对它的改动不会实时传播到其它端。
///     于是「客户端右键圣晶石抽到宝具卡」只写进客户端自己的 NobleDeck，主机端该玩家的
///     NobleDeck 仍停留在初始播种内容。之后 np_button 托管动作在所有端按【本地】NobleDeck
///     重放（候选列表取自 <c>noblePile.Cards</c>），各端候选不同 → 加入手牌的宝具不同 →
///     校验和分歧断线（现象：宝具牌只出现在客户端手牌、主机手牌没有）。
///     <para />
///     这里用 Sidecar 即时消息把「某玩家获得了一张宝具卡」广播到所有端（含主机），
///     各端据此在本地该玩家的 NobleDeck 内补记同一张卡，使全员 NobleDeck 保持一致，
///     从而让 np_button 动作在各端重放时得到相同结果。
/// </remarks>
public static class FgoNobleDeckSync
{
    /// <summary>某玩家获得一张宝具卡的通知。CardId 为规范卡 Id（<see cref="AbstractModel.Id" />）。</summary>
    public record NobleDeckAddMessage(ulong NetId, ModelId CardId);

    private static readonly RitsuLibSidecarMessageDescriptor<NobleDeckAddMessage> AddDescriptor = new(
        ModuleId: Entry.ModId,
        MessageKey: "fgo_nobledeck_add_v1",
        Serialize: static msg => JsonSerializer.SerializeToUtf8Bytes(msg),
        Deserialize: static bytes => JsonSerializer.Deserialize<NobleDeckAddMessage>(bytes)!,
        Delivery: RitsuLibSidecarDeliverySemantics.StableSync);

    private static IDisposable? _subscription;
    private static bool _handshakeSubscribed;

    /// <summary>
    ///     在 <see cref="Entry.Init" /> 中调用一次：订阅宝具卡加入消息，
    ///     并在会话握手完成时补播一次本机 NobleDeck，保证后加入的队友也能补齐。
    /// </summary>
    public static void Init()
    {
        _subscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(AddDescriptor, OnReceived);
        if (!_handshakeSubscribed)
        {
            _handshakeSubscribed = true;
            RitsuLibSidecarEvents.OnHandshakeCompleted(_ => ResyncLocalNobleDeck());
        }
    }

    /// <summary>
    ///     拥有者在本地把宝具卡加入 NobleDeck 后调用：把这次加入广播给其它端。
    ///     拥有者本机已通过 <c>CardPileCmd.Add</c> 完成加入，故收到自己的广播时会跳过。
    /// </summary>
    public static void NotifyAdd(Player? player, ModelId? cardId)
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null || player == null || cardId == null) return;

        var msg = new NobleDeckAddMessage(player.NetId, cardId);
        switch (netService.Type)
        {
            // 客户端 → 主机；主机收到后转发广播给其它队友
            case NetGameType.Client:
                RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, AddDescriptor, msg);
                break;
            // 主机 / 单机 → 直接广播（单机无对端，仅本机已加入生效）
            default:
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, AddDescriptor, msg);
                break;
        }

        Entry.Logger.Info($"[Fgo] NobleDeck add sync sent: netId={player.NetId}, card={cardId}");
    }

    /// <summary>
    ///     会话握手完成时调用：把【本机玩家】NobleDeck 的现有内容整体重播一次，
    ///     让后加入 / 漏收的队友补齐。ApplyAdd 幂等（同卡已存在即跳过），重复广播无副作用。
    /// </summary>
    public static void ResyncLocalNobleDeck()
    {
        var netService = RunManager.Instance?.NetService;
        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (netService == null || runState == null) return;

        var local = runState.Players.FirstOrDefault(p => p.NetId == netService.NetId);
        if (local == null) return;

        var pile = CardPile.Get(FgoEnums.NobleDeck, local);
        if (pile == null || pile.Cards.Count == 0) return;

        foreach (var card in pile.Cards)
            NotifyAdd(local, card.Id);
    }

    private static void OnReceived(RitsuLibSidecarTypedDispatchContext<NobleDeckAddMessage> context)
    {
        // 主机从客户端收到后转发广播给其它队友（广播只发对端、不回传主机，无环）。
        if (context.IsHostIngest)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                RunManager.Instance?.NetService, AddDescriptor, context.Message);

        ApplyAdd(context.Message.NetId, context.Message.CardId);
    }

    /// <summary>
    ///     把「某玩家获得了一张宝具卡」补记到本机该玩家的 NobleDeck。
    ///     拥有者本机已通过 <c>CardPileCmd.Add</c> 加入 → 跳过；同卡已存在 → 跳过（幂等）。
    /// </summary>
    private static void ApplyAdd(ulong netId, ModelId cardId)
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        // 自己就是该玩家：本地早已通过 CardPileCmd.Add 加入，避免重复。
        if (netService.NetId == netId) return;

        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;

        var player = runState.Players.FirstOrDefault(p => p.NetId == netId);
        if (player == null) return;

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null) return;

        if (pile.Cards.Any(c => c.Id == cardId)) return;

        var canonical = ModelDb.GetByIdOrNull<NobleCardModel>(cardId);
        if (canonical == null)
        {
            Entry.Logger.Warn($"[Fgo] NobleDeck sync: unknown card id '{cardId}'.");
            return;
        }

        pile.AddInternal(runState.CreateCard(canonical, player));
        Entry.Logger.Info($"[Fgo] NobleDeck add applied: netId={netId}, card={cardId}");
    }
}
