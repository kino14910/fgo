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
/// <remarks>
///     NobleDeck 是 RunPersistent 的 mod 牌堆，其持久化走
///     <c>PlayerRunSavedData</c>（见 RitsuLib 的 ModCardPilePersistence：主机权威、
///     仅在存/读档时快照），运行期间本机对它的改动不会实时传播到其它端。
///     于是「客户端右键圣晶石抽到宝具卡」只写进客户端自己的 NobleDeck，主机端该玩家的
///     NobleDeck 仍停留在初始播种内容。之后 np_button 托管动作在所有端按【本地】NobleDeck
///     重放（候选列表取自 <c>noblePile.Cards</c>），各端候选不同 → 加入手牌的宝具不同 →
///     校验和分歧断线。
///     <para />
///     同步采用「版本化整堆快照」：拥有者在本地每次改动 NobleDeck 后，自增该玩家牌堆的版本号，
///     并把【整堆内容】作为快照广播给所有端。各端按版本号收敛（低于已应用版本的快照直接丢弃），
///     整堆重建而非按单卡 Id 判重，因此同名重复宝具、乱序/重投送达都能正确还原。
///     接收端在 Godot 主线程上应用（<c>CardPile</c> 集合只能在主线程安全修改），
///     避开 Sidecar 回调所在的网络接收线程与游戏主线程之间的竞态。
/// </remarks>
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

    // 本机作为拥有者时的牌堆版本：每次本地改动自增，作为广播快照的版本号。
    private static readonly Dictionary<ulong, int> LocalVersion = new();

    // 本机已应用的牌堆版本（含其它玩家）：接收快照时低于此值的直接丢弃，按版本收敛而非按卡 Id 判重。
    private static readonly Dictionary<ulong, int> AppliedVersion = new();

    // 本机作为拥有者时的会话/局标识：每次新会话/新局刷新（真实时间派生的单调值）。
    // 接收端见到更大的 Epoch 即无条件接受快照，从而消除「跨进程重启/跨局重连」时版本号基数不一致、
    // 正当快照被误判为过时而丢弃的问题（netId 是跨进程稳定的 SteamId，旧版本基数不会自然归零）。
    private static long LocalEpoch = DateTime.UtcNow.Ticks;

    // 本机已应用的会话/局标识（含其它玩家）。
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
        // 会话边界刷新本机作为拥有者的 Epoch：即便本机进程刚重启（LocalVersion 归零），
        // 接收端也会因 Epoch 变大而无条件接受随后的快照。
        LocalEpoch = DateTime.UtcNow.Ticks;

        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        // 【关键】每个端都把自己拥有的 FGO 玩家牌堆作为权威内容主动推一次，即使本局内容从未变过（版本仍为 0）。
        // NobleDeck 只在本机播种 / 改动，而「只在改动时广播」有一个致命缺口：客户端牌堆的【基线内容】永远不会发给
        // 主机（播种不是一次 NotifyAdd）。于是主机的本地副本一旦与拥有者不一致（例如该端此刻解析不出角色、
        // 播种时机错过、或副本被空快照清空），就再没有任何机制能纠正它。
        // 后果：np_button 托管动作在所有端各按【本地】NobleDeck 取候选 —— 主机牌堆为空时会直接 return false
        // 跳过整段选择，而拥有者照常选牌并把宝具加进手牌 → 手牌数量分歧（曾表现为一端可选牌、另一端手牌少一张）。
        BroadcastOwnDecks();

        if (netService is NetHostGameService)
            BroadcastAllDecks(); // 顺带把已收到的其它玩家牌堆推给所有客户端
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
            var netService = RunManager.Instance?.NetService;
            var runState = RunManager.Instance?.DebugOnlyGetState();
            if (netService == null || runState == null) return;

            foreach (var player in runState.Players.Where(p =>
                         p.Character is FgoCharacter && p.NetId == netService.NetId))
            {
                // 空牌堆不补发基线：空内容不携带任何信息，却会把对端已正确的副本 Clear 成空
                // （例如握手事件早于读档播种触发）。NobleDeck 在正常玩法中不会被清空，
                // 真正因改动而清空的情况仍由 NotifyAdd → BroadcastOwnDeck 照常广播。
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
    /// </summary>
    public static void NotifyAdd(Player? player, ModelId? cardId)
    {
        try
        {
            if (player == null) return;
            BroadcastOwnDeck(player);
        }
        catch (Exception ex)
        {
            // 同步是「尽力而为」的旁路：任何异常都不能波及调用方的核心玩法（如右键扣费），
            // 否则一次同步故障就会把整段玩法逻辑吞掉（曾因此出现「右键不扣圣晶石」）。
            Entry.Logger.Warn($"[Fgo] NobleDeck snapshot send failed: {ex}");
        }
    }

    /// <summary>
    ///     会话握手完成时客户端调用：请求主机把全员 NobleDeck 整体重播，
    ///     弥补「客户端仅靠自己的本地牌堆、拿不到其它玩家在它加入前已加入的宝具」的缺口。
    /// </summary>
    private static void RequestFullResync()
    {
        var netService = RunManager.Instance?.NetService;
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
            var netService = RunManager.Instance?.NetService;
            var runState = RunManager.Instance?.DebugOnlyGetState();
            if (netService == null || runState == null) return;

            foreach (var player in runState.Players.Where(p => p.Character is FgoCharacter))
            {
                var pile = CardPile.Get(FgoEnums.NobleDeck, player);
                if (pile == null) continue;

                var isOwner = netService.NetId == player.NetId;
                long epoch;
                int version;
                if (isOwner)
                {
                    // 拥有者用本机权威 Epoch。即便 version=0（本局还没通过 NotifyAdd 加过卡）也照播：
                    // Epoch 是新值，接收端会无条件接受，从而把初始播种内容一并推给客户端。
                    epoch = LocalEpoch;
                    version = LocalVersion.GetValueOrDefault(player.NetId);
                }
                else
                {
                    // 主机视角下的客户端玩家：用主机已记录的该玩家 Epoch/版本；从未收到过则无权威内容可播。
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
        // 注意：不能用 `++LocalVersion[key]` —— Dictionary 索引器读缺失 key 会抛 KeyNotFoundException，
        // 该异常发生在调用方（SaintQuartz 右键）扣费之前，会导致整段右键逻辑中断（表现为“点了不扣圣晶石”）。
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
        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null) return;

        var msg = new NobleDeckSnapshotMessage(
            player.NetId, LocalEpoch, version, pile.Cards.Select(c => c.Id.ToString()).ToList());

        switch (netService.Type)
        {
            // 客户端 → 主机；主机收到后转发广播给其它队友
            case NetGameType.Client:
                RitsuLibSidecarTypedMessageRegistry.SendToHost(netService, SnapshotDescriptor, msg);
                break;
            // 主机 / 单机 → 直接广播（单机无对端，本机已加入生效）
            default:
                RitsuLibSidecarTypedMessageRegistry.Broadcast(netService, SnapshotDescriptor, msg);
                break;
        }

        Entry.Logger.Info(
            $"[Fgo] NobleDeck snapshot sent: netId={player.NetId}, epoch={LocalEpoch}, version={version}, cards={msg.CardIds.Count}");
    }

    private static void OnResyncReceived(RitsuLibSidecarTypedDispatchContext<NobleDeckResyncRequest> context)
    {
        // 仅主机在收到客户端请求时重播全员牌组；客户端忽略（它已通过 Broadcast 接收）。
        if (context.IsHostIngest)
            BroadcastAllDecks();
    }

    private static void OnSnapshotReceived(RitsuLibSidecarTypedDispatchContext<NobleDeckSnapshotMessage> context)
    {
        // 主机从客户端收到后转发广播给其它队友（广播不回传发送方，无环）。
        if (context.IsHostIngest)
            RitsuLibSidecarTypedMessageRegistry.Broadcast(
                RunManager.Instance?.NetService, SnapshotDescriptor, context.Message);

        // 调度到 Godot 主线程：CardPile 集合只能在主线程安全修改，避开网络接收线程竞态。
        Callable.From(() => ApplySnapshot(context.Message)).CallDeferred();
    }

    /// <summary>
    ///     在 Godot 主线程上按 (Epoch, Version) 收敛地重建某玩家 NobleDeck。
    ///     拥有者本机已通过 CardPileCmd.Add 加入 → 跳过；旧会话（Epoch 更小）或同会话旧版本 → 丢弃
    ///     （乱序/重投送达）；否则整堆重建（保留同名重复卡），并记下已应用 Epoch/版本。
    /// </summary>
    private static void ApplySnapshot(NobleDeckSnapshotMessage msg)
    {
        var netService = RunManager.Instance?.NetService;
        if (netService == null) return;

        // 自己就是该玩家：本地早已加入（本机是权威），跳过避免覆盖本地真理。
        if (netService.NetId == msg.NetId) return;

        // 按 (Epoch, Version) 收敛：Epoch 更大 → 无条件接受（拥有者新会话/新局）；Epoch 相同才比版本；
        // Epoch 更小 → 旧会话的迟到快照，丢弃。
        if (AppliedEpoch.TryGetValue(msg.NetId, out var appliedEpoch))
        {
            if (msg.Epoch < appliedEpoch) return;
            if (msg.Epoch == appliedEpoch &&
                AppliedVersion.TryGetValue(msg.NetId, out var appliedVersion) &&
                msg.Version <= appliedVersion) return;
        }

        var runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null) return;

        var player = runState.Players.FirstOrDefault(p => p.NetId == msg.NetId);
        if (player == null) return;

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null) return;

        // 按快照整体重建（Clear + 逐张加回），保留同名重复卡；幂等于版本号。
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