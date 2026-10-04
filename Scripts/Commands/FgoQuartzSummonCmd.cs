using System.Text;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Relics;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;

namespace Fgo.Scripts.Commands;

/// <summary>
///     托管网络动作: 圣晶石 / 召唤券抽取宝具的【结果提交】。
///     <para>
///         抽取界面（等待玩家点击）绝不能放进同步动作里 —— 那会让该 GameAction 一直不结束，
///         而 ActionExecutor 是全局串行的：此后所有玩家（包括队友）的后续动作都排不上，
///         典型表现就是「A 点圣晶石后 A、B 点房间 → B 黑屏卡死」。
///         因此界面改由本地在同步动作【之外】打开，玩家选中的卡以 Id 通过本动作广播，
///         各端按 Id 确定性重建同一张卡、加入 NobleDeck 并扣除计数。
///     </para>
///     必须在 Entry.Init 注册，保证任何 peer 发起前本端已注册。
/// </summary>
public static class FgoQuartzSummonCmd
{
    /// <summary>地图 / 非战斗场景使用（与 RitsuLib 右键动作选取动作类型的规则一致）。</summary>
    internal static readonly RitsuLibManagedNetActionDescriptor<string> NonCombatDescriptor = new(
        Entry.ModId,
        "quartz_summon_noncombat",
        static id => Encoding.UTF8.GetBytes(id),
        static bytes => Encoding.UTF8.GetString(bytes),
        ExecuteManaged,
        GameActionType.NonCombat);

    /// <summary>战斗中（出牌阶段）使用。</summary>
    internal static readonly RitsuLibManagedNetActionDescriptor<string> CombatDescriptor = new(
        Entry.ModId,
        "quartz_summon_combat",
        static id => Encoding.UTF8.GetBytes(id),
        static bytes => Encoding.UTF8.GetString(bytes),
        ExecuteManaged,
        GameActionType.CombatPlayPhaseOnly);

    /// <summary>
    ///     抽取界面选完后的提交入口: 仅本机玩家调用（调用方需保证 LocalContext.IsMe）。
    ///     返回 false 表示动作未能入队（如战斗中非出牌阶段），调用方应放弃本次抽取。
    /// </summary>
    public static bool Request(Player player, ModelId cardId)
    {
        var runManager = RunManager.Instance;
        if (runManager == null) return false;

        if (!CombatManager.Instance.IsInProgress)
            return RitsuLibManagedNetActions.Request(runManager, NonCombatDescriptor, cardId.ToString());

        if (runManager.ActionQueueSynchronizer.CombatState != ActionSynchronizerCombatState.PlayPhase)
            return false;

        return RitsuLibManagedNetActions.Request(runManager, CombatDescriptor, cardId.ToString());
    }

    /// <summary>当前是否可以提交（用于在弹出界面前判断，避免选完才发现发不出去）。</summary>
    public static bool CanRequestNow()
    {
        var runManager = RunManager.Instance;
        if (runManager == null) return false;
        if (!CombatManager.Instance.IsInProgress) return true;
        return runManager.ActionQueueSynchronizer.CombatState == ActionSynchronizerCombatState.PlayPhase;
    }

    private static async Task ExecuteManaged(RitsuLibManagedNetActionContext<string> context)
    {
        var player = context.Player;

        ModelId cardId;
        try
        {
            cardId = ModelId.Deserialize(context.Message);
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: malformed card id '{context.Message}': {ex.Message}");
            return;
        }

        if (ModelDb.GetByIdOrNull<NobleCardModel>(cardId) is not { } canonical)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: unknown noble card '{cardId}'.");
            return;
        }

        var pile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (pile == null)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: NobleDeck missing for netId={player.NetId}.");
            return;
        }

        // 重复提交（连点 / 重发）守卫：已拥有该宝具则整段跳过，避免重复加卡与重复扣费。
        if (pile.Cards.Any(c => c.Id == canonical.Id))
        {
            Entry.Logger.Warn($"[Fgo] quartz summon skipped: {canonical.Id.Entry} already in NobleDeck of netId={player.NetId}.");
            return;
        }

        var relic = FindQuartzRelic(player);
        if (relic == null)
        {
            Entry.Logger.Warn($"[Fgo] quartz summon rejected: no quartz relic for netId={player.NetId}.");
            return;
        }

        // 计数不足（各端状态应一致，客户端被覆盖时也走同一守卫）→ 不白嫖。
        if (relic.QuartzCount < SaintQuartz.CostPerChoice)
        {
            Entry.Logger.Warn(
                $"[Fgo] quartz summon rejected: netId={player.NetId} count={relic.QuartzCount} < {SaintQuartz.CostPerChoice}.");
            return;
        }

        // NobleDeck 是 RunPersistent 牌堆，用 RunState.CreateCard 而非 CombatState.CreateCard，
        // 使本动作在地图上也能执行（加入的卡不进战斗 pile，无需注册到 CombatState）。
        var card = player.RunState.CreateCard(canonical, player);
        var result = await CardPileCmd.Add(card, pile);
        if (result is not { success: true })
        {
            Entry.Logger.Warn($"[Fgo] quartz summon failed: could not add {canonical.Id.Entry} for netId={player.NetId}.");
            return;
        }

        // 扣费放在加卡成功之后：各端都执行（主机侧的写入是权威值，客户端的写入会被主机的广播覆盖），
        // 因此不存在「客户端扣了主机没扣」的偏差。
        relic.SpendQuartz(SaintQuartz.CostPerChoice);

        // NobleDeck 是 RunPersistent 牌堆，运行期改动不会自动传播 → 显式广播补记。
        FgoNobleDeckSync.NotifyAdd(player, card.Id);
        FgoQuartzSync.NotifyLocalCount(player);

        Entry.Logger.Info($"[Fgo] quartz summon: netId={player.NetId} gained {card.Id.Entry}");
    }

    private static FgoRelic? FindQuartzRelic(Player player)
    {
        return (FgoRelic?)player.GetRelic<SaintQuartz>() ?? player.GetRelic<SummonTicket>();
    }
}
