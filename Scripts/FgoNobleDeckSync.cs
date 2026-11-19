using System.Text;
using System.Text.Json;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using Godot;
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
/// <remarks> NobleDeck 是 RunPersistent，改动不会实时同步（持久化只在存读档），故用版本化整堆快照广播；各端按版本收敛、在 Godot 主线程应用。</remarks>
public static class FgoNobleDeckSync
{
    private static readonly RitsuLibSidecarMessageDescriptor<NobleDeckSnapshotMessage> SnapshotDescriptor = new(
        Entry.ModId,
        "fgo_nobledeck_snapshot_v1",
        static msg => Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new SnapshotDto(msg.NetId, msg.Epoch, msg.Version, msg.CardIds))),
        static bytes =>
        {
            var dto = JsonSerializer.Deserialize<SnapshotDto>(
                Encoding.UTF8.GetString(bytes));
            return new NobleDeckSnapshotMessage(dto!.NetId, dto.Epoch, dto.Version, dto.CardIds);
        });

    private static readonly RitsuLibSidecarMessageDescriptor<NobleDeckResyncRequest> ResyncDescriptor = new(
        Entry.ModId,
        "fgo_nobledeck_resync_v1",
        static _ => Array.Empty<byte>(),
        static _ => new NobleDeckResyncRequest());

    // 接收快照时低于已应用版本的直接丢弃，按版本收敛而非按卡 Id 判重。
    private static readonly Dictionary<ulong, int> LocalVersion = new();

    private static readonly Dictionary<ulong, int> AppliedVersion = new();

    private static long LocalEpoch = DateTime.UtcNow.Ticks;

    private static readonly Dictionary<ulong, long> AppliedEpoch = new();

    private static IDisposable? _snapshotSubscription;
    private static IDisposable? _resyncSubscription;
    private static bool _handshakeSubscribed;

    /// <summary>
    ///     在 <see cref="Entry.Init" /> 中调用一次：订阅整堆快照 / 全量重播请求消息，
    ///     并在会话握手完成时由主机广播全员 NobleDeck、客户端主动请求主机重播。
    /// </summary>
    public static void Init()
    {
        _snapshotSubscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(SnapshotDescriptor, OnSnapshotReceived);
        _resyncSubscription ??= RitsuLibSidecarTypedMessageRegistry.Subscribe(ResyncDescriptor, OnResyncReceived);
        if (!_handshakeSubscribed)
        {
            _handshakeSubscribed = true;
            RitsuLibSidecarEvents.OnHandshakeCompleted(_ => OnHandshake());
        }
    }

    /// <summary>
    ///     每局开始（RunStarted / RunLoaded）由 Entry 调用：刷新 Epoch 并重播全员牌堆，
    ///     保证新局 / 重连各端从同一基线收敛（不刷新 Epoch 会让跨重启残留的旧版本基数挡住新快照）。
    /// </summary>
    public static void OnRunStarted()
    {
        OnHandshake();
    }

    private static void OnHandshake()
    {
        // 每端都把自已拥有的牌堆作为权威内容主动推一次，即使本局没改动：「只在改动时广播」无法把基线内容发给主机，会致手牌分歧。
        LocalEpoch = DateTime.UtcNow.Ticks;

        var netService = RunManager.Instance.NetService;

        BroadcastOwnDecks();

        if (netService is NetHostGameService)
            BroadcastAllDecks();
        else if (netService is NetClientGameService)
            RequestFullResync(); // 再向主机请求一次全量，覆盖中途加入前已存在的牌堆
    }

    /// <summary>
    ///     把【本机拥有的】FGO 玩家 NobleDeck 作为权威内容各推一次（即使本局没改动过、版本仍为 0）。
    ///     与 <see cref="BroadcastOwnDeck" /> 的区别：不递增版本号 —— 内容并没有变化，这里只是补发基线，
    ///     让所有端都能拿到拥有者的真实牌堆内容，而不是各自本地播种的猜测。
    /// </summary>
    private static void BroadcastOwnDecks()
    {
        try
        {
            var netService = RunManager.Instance.NetService;
            var runState = RunManager.Instance.DebugOnlyGetState();
            if (runState == null) return;

            foreach (var player in runState.Players.Where(p =>
                         p.Character is FgoCharacter && p.NetId == netService.NetId))
            {
                var pile = CardPile.Get(FgoEnums.NobleDeck, player);
                if (pile == null || pile.IsEmpty) continue;

                SendOwnDeckSnapshot(player, LocalVersion.GetValueOrDefault(player.NetId));
            }
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Fgo] NobleDeck own-baseline broadcast failed: {ex}");
        }
    }

    /// <summary>
    ///     拥有者在本地把宝具卡加入 NobleDeck 后调用：自增版本并广播整堆快照。
    ///     拥有者本机已通过 <c>CardPileCmd.Add</c> 完成加入，收到的自己的快照会按版本跳过，不重复。
    ///     非拥有者端执行同一动作时本地也会加卡，但那份只是副本：若它也以拥有者身份广播，
    ///     会带着本机的 Epoch/Version 冒充权威内容，覆盖真正的拥有者牌堆（Epoch 最大的端胜出），
    ///     且抬高 AppliedEpoch 后让拥有者的后续快照被永久丢弃 → 各端候选列表错位 → np_button 分歧。
    /// </summary>
    public static void NotifyAdd(Player? player, ModelId? cardId)
    {
        try
        {
            if (player == null) return;

            if (RunManager.Instance.NetService is { } netService && netService.NetId != player.NetId)
                return;

            BroadcastOwnDeck(player);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Fgo] NobleDeck snapshot send failed: {ex}");
        }
    }

    /// <summary>
    ///     会话握手完成时客户端调用：请求主机把全员 NobleDeck 整体重播，
    ///     弥补「客户端仅靠自己的本地牌堆、拿不到其它玩家在它加入前已加入的宝具」的缺口。
    /// </summary>
    private static void RequestFullResync()
    {
        var netService = RunManager.Instance.NetService;
        if (netService is not NetClientGameService) return;

        RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, ResyncDescriptor, new NobleDeckResyncRequest());
        Entry.Logger.Info("[Fgo] NobleDeck full resync requested from host.");
    }

    /// <summary>
    ///     主机在握手完成时调用：把【所有 FGO 玩家】NobleDeck 的现有内容整体广播一次。
    ///     拥有者用本机权威版本（<see cref="LocalVersion" />），非拥有者用主机已应用的版本
    ///     （<see cref="AppliedVersion" />，主机持续接收并重建了这些牌堆的本地副本），
    ///     保证中途加入 / 漏收的客户端也能补齐任意玩家在它加入前已加入的宝具。
    /// </summary>
    private static void BroadcastAllDecks()
    {
        try
        {
            var netService = RunManager.Instance.NetService;
            var runState = RunManager.Instance.DebugOnlyGetState();
            if (runState == null) return;

            foreach (var player in runState.Players.Where(p => p.Character is FgoCharacter))
            {
                var pile = CardPile.Get(FgoEnums.NobleDeck, player);
                if (pile == null) continue;

                var isOwner = netService.NetId == player.NetId;
                long epoch;
                int version;
                if (isOwner)
                {
                    epoch = LocalEpoch;
                    version = LocalVersion.GetValueOrDefault(player.NetId);
                }
                else
                {
                    epoch = AppliedEpoch.GetValueOrDefault(player.NetId);
                    if (epoch == 0) continue;
                    version = AppliedVersion.GetValueOrDefault(player.NetId);
                }

                var msg = new NobleDeckSnapshotMessage(
                    player.NetId, epoch, version, pile.Cards.Select(c => c.Id.ToString()).ToList());
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, SnapshotDescriptor, msg);
            }

            Entry.Logger.Info("[Fgo] NobleDeck full resync broadcasted to clients.");
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Fgo] NobleDeck full resync failed: {ex}");
        }
    }

    /// <summary>
    ///     拥有者本地改动后把整堆快照广播出去。客户端发往主机（主机再转发给其它队友），
    ///     主机/单机直接广播。
    /// </summary>
    private static void BroadcastOwnDeck(Player player)
    {
        // 不能用 ++LocalVersion[key]：Dictionary 索引器读缺失 key 会抛 KeyNotFoundException，中断调用方结算。
        var version = LocalVersion.GetValueOrDefault(player.NetId) + 1;
        LocalVersion[player.NetId] = version;
        SendOwnDeckSnapshot(player, version);
    }

    /// <summary>
    ///     把某个玩家 NobleDeck 的当前内容作为整堆快照发出去（客户端发往主机，主机 / 单机直接广播）。
    ///     版本号由调用方决定：本地改动用递增后的版本，握手补发基线用当前版本（可能仍为 0）。
    /// </summary>
    private static void SendOwnDeckSnapshot(Player player, int version)
    {
        var netService = RunManager.Instance.NetService;

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null) return;

        var msg = new NobleDeckSnapshotMessage(
            player.NetId, LocalEpoch, version, pile.Cards.Select(c => c.Id.ToString()).ToList());

        switch (netService.Type)
        {
            case NetGameType.Client:
                RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, SnapshotDescriptor, msg);
                break;
            default:
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, SnapshotDescriptor, msg);
                break;
        }

        Entry.Logger.Info(
            $"[Fgo] NobleDeck snapshot sent: netId={player.NetId}, epoch={LocalEpoch}, version={version}, cards={msg.CardIds.Count}");
    }

    private static void OnResyncReceived(RitsuLibSidecarTypedDispatchContext<NobleDeckResyncRequest> context)
    {
        if (context.IsHostIngest)
            BroadcastAllDecks();
    }

    private static void OnSnapshotReceived(RitsuLibSidecarTypedDispatchContext<NobleDeckSnapshotMessage> context)
    {
        if (context.IsHostIngest)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                RunManager.Instance.NetService, SnapshotDescriptor, context.Message);

        Callable.From(() => ApplySnapshot(context.Message)).CallDeferred();
    }

    /// <summary>
    ///     在 Godot 主线程上按 (Epoch, Version) 收敛地重建某玩家 NobleDeck。
    ///     拥有者本机已通过 CardPileCmd.Add 加入 → 跳过；旧会话（Epoch 更小）或同会话旧版本 → 丢弃
    ///     （乱序/重投送达）；否则整堆重建（保留同名重复卡），并记下已应用 Epoch/版本。
    /// </summary>
    private static void ApplySnapshot(NobleDeckSnapshotMessage msg)
    {
        var netService = RunManager.Instance.NetService;

        if (netService.NetId == msg.NetId) return;

        if (AppliedEpoch.TryGetValue(msg.NetId, out var appliedEpoch))
        {
            if (msg.Epoch < appliedEpoch) return;
            if (msg.Epoch == appliedEpoch &&
                AppliedVersion.TryGetValue(msg.NetId, out var appliedVersion) &&
                msg.Version <= appliedVersion) return;
        }

        var runState = RunManager.Instance.DebugOnlyGetState();

        var player = runState?.Players.FirstOrDefault(p => p.NetId == msg.NetId);
        if (player == null) return;

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null) return;

        pile.Clear(true);
        foreach (var cardId in msg.CardIds)
        {
            var id = ModelId.Deserialize(cardId);
            var canonical = ModelDb.GetByIdOrNull<NobleCardModel>(id);
            if (canonical == null)
            {
                Entry.Logger.Warn($"[Fgo] NobleDeck sync: unknown card id '{cardId}'.");
                continue;
            }

            pile.AddInternal(runState.CreateCard(canonical, player));
        }

        AppliedEpoch[msg.NetId] = msg.Epoch;
        AppliedVersion[msg.NetId] = msg.Version;
        Entry.Logger.Info(
            $"[Fgo] NobleDeck snapshot applied: netId={msg.NetId}, epoch={msg.Epoch}, version={msg.Version}");
    }

    /// <summary>
    ///     整堆快照：拥有者 NetId、会话/局标识 Epoch、单调递增版本号、完整卡 Id 列表
    ///     （可含重复，表示同名宝具多张）。卡 Id 以字符串传输（<see cref="ModelId" /> 的规范文本形式），
    ///     避免 record 直接 JSON 化的坑。
    ///     <para />
    ///     Epoch 用于消除「跨进程重启 / 跨局重连」时版本号基数不一致的问题：拥有者在每次新会话 / 新局取一个
    ///     新的（真实时间派生、单调递增的）Epoch，接收端见到更大的 Epoch 即无条件接受该快照。
    /// </summary>
    public record NobleDeckSnapshotMessage(ulong NetId, long Epoch, int Version, List<string> CardIds);

    /// <summary>客户端请求主机把全员 NobleDeck 整体重播一次（覆盖中途加入前已加入的卡）。</summary>
    public record NobleDeckResyncRequest;

    private sealed record SnapshotDto(ulong NetId, long Epoch, int Version, List<string> CardIds);
}