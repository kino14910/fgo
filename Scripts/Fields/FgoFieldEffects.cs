using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Fields;

/// <summary>
///     场地效果的结算实现。由 <c>FgoBattleHooks</c> 在战斗钩子上调用。
/// </summary>
/// <remarks>
///     结算原则：在「结算点」查询场地，而非施加时把数值烧死到单位身上——中途召唤/复活的单位天然被覆盖，不必额外挂生成钩子。
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

    /// <summary>〔都市〕我方回合开始时，每名玩家获得的能量。</summary>
    public const decimal CityEnergy = 1m;

    /// <summary>〔都市〕我方回合开始时，没有「人工制品」的敌人获得的人工制品层数。</summary>
    public const decimal CityArtifactStacks = 1m;

    /// <summary>
    ///     我方（玩家方）回合开始: 先依次结算「水边」「黑暗」「森林」「阳光照射」「都市」，最后让有时限的场地递减。
    /// </summary>
    /// <remarks>
    ///     先结算再递减：层数 N = 「回合开始」类结算 N 次，进场场地不会在首回合白掉一层。
    ///     用 <see cref="BlockingPlayerChoiceContext" />（场地施加无玩家选择，无阻塞风险）；能力来源是战场、applier 传 null（<c>PowerCmd.Apply</c> 显式支持）。
    ///     <c>await FgoField.TickDown</c> 的 Task 恒已完成（见 <see cref="FgoField" /> 的 remarks），递减同步落地。
    /// </remarks>
    public static async Task OnPlayerSideTurnStart(ICombatState combat)
    {
        // 重置〔阳光照射〕的「本回合首次攻击牌」标记：每回合重新计一次。
        var sunlightTurn = FgoField.Of(combat);
        if (sunlightTurn != null)
            sunlightTurn.SunlightAttackPlayedThisTurn = false;

        if (FgoField.Has(combat, FgoFieldId.Waterside))
            foreach (var creature in LivingUnits(combat))
                await CreatureCmd.GainBlock(creature, WatersideBlock, ValueProp.Unpowered, null);

        var choiceContext = new BlockingPlayerChoiceContext();
        var units = LivingUnits(combat);
        if (units.Count > 0)
        {
            if (FgoField.Has(combat, FgoFieldId.Darkness))
                await ApplyFieldPower<BlindPower>(choiceContext, units, DarknessBlindStacks);

            if (FgoField.Has(combat, FgoFieldId.Forest))
            {
                var wounded = units.Where(static c => c.CurrentHp < c.MaxHp * ForestRegenHpRatio).ToList();
                await ApplyFieldPower<RegenPower>(choiceContext, wounded, ForestRegenAmount);
            }

            if (FgoField.Has(combat, FgoFieldId.Sunlight))
                foreach (var ally in LivingAllies(combat))
                {
                    var vigor = ally.GetPower<VigorPower>()?.Amount ?? 0;
                    await PowerCmd.Apply<VigorPower>(choiceContext, ally, vigor + SunlightTurnStartVigor, null, null);
                }
        }

        // 最后结算：人工制品会抵消之后施加的减益，不能抢在〔黑暗〕等场地之前。
        if (FgoField.Has(combat, FgoFieldId.City))
        {
            foreach (var player in combat.Players)
                await PlayerCmd.GainEnergy(CityEnergy, player);

            var unprotected = combat.Enemies
                .Where(static e => e.IsAlive && e.GetPower<ArtifactPower>() is not { Amount: > 0 })
                .ToList();
            await ApplyFieldPower<ArtifactPower>(choiceContext, unprotected, CityArtifactStacks);
        }

        await FgoField.TickDown(combat);
    }

    /// <summary>
    ///     我方（玩家方）回合结束: 结算「燃烧」（伤害）。
    /// </summary>
    /// <remarks>
    ///     伤害用 <see cref="ValueProp.Unpowered" />（来自效果而非攻击）结算：不吃力量/易伤、不触发暴击与宝具值，但可被格挡。
    ///     〔阳光照射〕已改到回合开始结算（见 <see cref="OnPlayerSideTurnStart" />），是唯一只对己方（含随从）生效的场地；其余场地对敌我所有单位生效。
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
    ///     仅作用于玩家方——怪物攻击不走 <c>CardPlay</c>。
    ///     「本回合第一次」由 <see cref="FgoFieldState.SunlightAttackPlayedThisTurn" /> 标记，<see cref="OnPlayerSideTurnStart" /> 每回合重置、命中置位，同回合后续攻击牌不再触发。
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
    ///     每名玩家回合开始时: 自愈式补齐〔虚数空间〕专用牌、清理残留状态。
    /// </summary>
    /// <remarks>
    ///     必须挂 <c>AfterPlayerTurnStart</c>（每名玩家各一次）而非 side 级：虚数空间是按玩家持有的单人效果，
    ///     各人独立进出，不能由战场级钩子统一裁决。这里不计时，虚数空间不会自动到期。
    /// </remarks>
    public static Task OnPlayerTurnStart(Player player)
    {
        return FgoVoidHand.OnPlayerTurnStart(player);
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

    /// <summary>场地施加能力：抵消 <c>PowerCmd.Apply</c> 对敌人首次施加的多人缩放，保证敌我层数一致。</summary>
    private static async Task ApplyFieldPower<T>(PlayerChoiceContext choiceContext,
        IEnumerable<Creature> targets, decimal amount) where T : PowerModel
    {
        foreach (var target in targets)
        {
            var before = target.GetPower<T>()?.Amount ?? 0;
            var power = await PowerCmd.Apply<T>(choiceContext, target, amount, null, null);
            if (power == null) continue;

            var delta = power.Amount - before;
            if (delta > amount)
                await PowerCmd.ModifyAmount(choiceContext, power, amount - delta, null, null);
        }
    }
}