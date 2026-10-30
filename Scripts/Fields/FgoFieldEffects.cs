using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Fields;

/// <summary>
///     场地效果的结算实现。由 <c>FgoBattleHooks</c> 在战斗钩子上调用。
/// </summary>
/// <remarks>
///     结算原则: 在「结算点」查询场地，而不是在施加场地时把数值烧死到单位身上。
///     这样中途召唤、复活的单位天然被覆盖，不必额外挂生成钩子；
///     反之若在施加时枚举单位逐个挂状态，漏掉未出场的敌人几乎无法避免。
///     <para>
///         //TODO 〔都市〕(<see cref="FgoFieldId.City" />) 的战场效果尚未确定:
///         目前只支持被施加与被查询，两个结算点都没有它的分支。
///         补效果时在 <see cref="OnPlayerSideTurnStart" /> / <see cref="OnPlayerSideTurnEnd" /> 里各加一段即可。
///     </para>
/// </remarks>
public static class FgoFieldEffects
{
    /// <summary>〔水边〕我方回合开始时，每个单位获得的格挡。</summary>
    public const decimal WatersideBlock = 3m;

    /// <summary>〔燃烧〕我方回合结束时，每个单位受到的伤害。</summary>
    public const decimal BurningDamage = 3m;

    /// <summary>〔黑暗〕我方回合开始时，每个单位获得的「盲目」层数。</summary>
    public const decimal DarknessBlindStacks = 1m;

    /// <summary>〔森林〕我方回合开始时，符合条件单位获得的「再生」层数。</summary>
    public const decimal ForestRegenAmount = 3m;

    /// <summary>〔森林〕获得再生的生命值门槛（占最大生命值的比例，低于此值才获得）。</summary>
    public const decimal ForestRegenHpRatio = 0.5m;

    /// <summary>〔阳光照射〕我方回合开始时，己方单位在翻倍既有「活力」之外额外获得的「活力」层数。</summary>
    public const decimal SunlightTurnStartVigor = 4m;

    /// <summary>
    ///     我方（玩家方）回合开始: 先让有时限的场地递减，再依次结算「水边」「黑暗」「森林」「阳光照射」。
    /// </summary>
    /// <remarks>
    ///     先递减再结算: 层数 N 表示该场地存在 N 个我方回合（施放当回合算第 1 个）。
    ///     由于施放当回合的回合开始已经过去，「回合开始」类结算从下一回合起才触发
    ///     （共 N-1 次）；「回合结束」类结算当回合就会触发（共 N 次）。
    ///     <para />
    ///     <see cref="MegaCrit.Sts2.Core.Models.AbstractModel.AfterSideTurnStart" /> 不带
    ///     <see cref="PlayerChoiceContext" />，而施加能力需要它；这里用
    ///     <see cref="BlockingPlayerChoiceContext" />（场地施加不产生玩家选择，不存在阻塞风险）。
    ///     <para />
    ///     能力来源是战场本身而非某个生物，因此 <c>applier</c> 传 null。
    ///     <c>PowerCmd.Apply</c> 显式支持 null applier（内部只跳过「施加方修正」那一段）。
    ///     <para />
    ///     <c>await FgoField.TickDown</c> 的 Task 恒为已完成（见 <see cref="FgoField" /> 的 remarks），
    ///     因此递减仍是同步落地的，紧随其后的 <c>Has</c> 判定看到的一定是递减后的状态。
    /// </remarks>
    public static async Task OnPlayerSideTurnStart(ICombatState combat)
    {
        await FgoField.TickDown(combat);

        // 重置〔阳光照射〕的「本回合首次攻击牌」标记：每回合重新计一次。
        var sunlightTurn = FgoField.Of(combat);
        if (sunlightTurn != null)
            sunlightTurn.SunlightAttackPlayedThisTurn = false;

        // 递减之后立刻结算〔虚数空间〕的到期退出：side 级钩子每轮只跑一次，
        // 因此「场次结束」会在同一份复制动作里对所有人同时发生（见 FgoVoidHand.ExitIfInactive）。
        await FgoVoidHand.ExitIfInactive(combat);

        if (FgoField.Has(combat, FgoFieldId.Waterside))
            foreach (var creature in LivingUnits(combat))
                await CreatureCmd.GainBlock(creature, WatersideBlock, ValueProp.Unpowered, null);

        var choiceContext = new BlockingPlayerChoiceContext();
        var units = LivingUnits(combat);
        if (units.Count <= 0) return;

        if (FgoField.Has(combat, FgoFieldId.Darkness))
            await PowerCmd.Apply<BlindPower>(choiceContext, units, DarknessBlindStacks, null, null);

        if (FgoField.Has(combat, FgoFieldId.Forest))
        {
            var wounded = units.Where(static c => c.CurrentHp < c.MaxHp * ForestRegenHpRatio).ToList();
            if (wounded.Count > 0)
                await PowerCmd.Apply<RegenPower>(choiceContext, wounded, ForestRegenAmount, null, null);
        }

        if (FgoField.Has(combat, FgoFieldId.Sunlight))
            foreach (var ally in LivingAllies(combat))
            {
                var vigor = ally.GetPower<VigorPower>()?.Amount ?? 0;
                await PowerCmd.Apply<VigorPower>(choiceContext, ally, vigor + SunlightTurnStartVigor, null, null);
            }
    }

    /// <summary>
    ///     我方（玩家方）回合结束: 结算「燃烧」（伤害）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         伤害用 <see cref="ValueProp.Unpowered" />（来自能力/效果而非攻击）结算:
    ///         不吃力量、不吃易伤，也不会触发暴击、宝具值等攻击侧逻辑；可被格挡。
    ///     </para>
    ///     <para>
    ///         〔阳光照射〕已改到回合开始结算（见 <see cref="OnPlayerSideTurnStart" />），
    ///         是唯一**只对己方单位生效**的场地（<c>combat.Allies</c>，含随从），
    ///         其余场地均对战场上所有单位（含敌方）生效。
    ///     </para>
    /// </remarks>
    public static async Task OnPlayerSideTurnEnd(PlayerChoiceContext choiceContext, ICombatState combat)
    {
        if (FgoField.Has(combat, FgoFieldId.Burning))
        {
            var targets = LivingUnits(combat);
            if (targets.Count > 0)
                await CreatureCmd.Damage(choiceContext, targets, BurningDamage, ValueProp.Unpowered,
                    null, null, null);
        }
    }

    /// <summary>
    ///     任意玩家打出一张牌时: 〔阳光照射〕下，本回合**第一次**打出攻击牌的玩家获得等同于剩余层数的「活力」。
    /// </summary>
    /// <remarks>
    ///     这一段天然只作用于玩家方——怪物攻击不走 <c>CardPlay</c>。
    ///     <para />
    ///     「本回合第一次」由 <see cref="FgoFieldState.SunlightAttackPlayedThisTurn" /> 标记，
    ///     在 <see cref="OnPlayerSideTurnStart" /> 每回合重置；命中后立即置位，同回合后续攻击牌不再触发。
    /// </remarks>
    public static async Task OnCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card is not { Type: CardType.Attack }) return;

        var creature = cardPlay.Player.Creature;
        var combat = creature.CombatState;
        var sunlight = FgoField.StacksOf(combat, FgoFieldId.Sunlight);
        if (sunlight <= 0) return;

        var fieldState = FgoField.Of(combat);
        if (fieldState == null || fieldState.SunlightAttackPlayedThisTurn) return;

        fieldState.SunlightAttackPlayedThisTurn = true;
        await PowerCmd.Apply<VigorPower>(choiceContext, creature, sunlight, creature, null);
    }

    /// <summary>
    ///     每名玩家回合开始时: 对〔虚数空间〕的「替换手牌」状态做一次自愈式对账。
    /// </summary>
    /// <remarks>
    ///     这一段刻意挂在 <c>AfterPlayerTurnStart</c>（每名玩家各一次）而不是 side 级钩子:
    ///     状态是**按玩家**的（每个玩家的手牌/额外手牌各一份），按玩家结算才对得上。
    ///     <para />
    ///     实际进出由 <see cref="FgoVoidHand" /> 负责；这里只处理"该修就修"的两种情况，
    ///     正常路径是进入那一刻（<c>GreatVoidSeaBattle</c> 出牌时）就已经替换好了。
    /// </remarks>
    public static Task OnPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var combat = player.Creature.CombatState;
        return combat == null ? Task.CompletedTask : FgoVoidHand.Reconcile(combat, player);
    }

    /// <summary>战场上的所有存活单位（敌我双方）。</summary>
    private static List<Creature> LivingUnits(ICombatState combat)
    {
        return combat.Creatures.Where(static c => c.IsAlive).ToList();
    }

    /// <summary>己方（玩家方）的存活单位，含随从。</summary>
    private static List<Creature> LivingAllies(ICombatState combat)
    {
        return combat.Allies.Where(static c => c.IsAlive).ToList();
    }
}