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

public static class FgoFieldEffects
{
    public const decimal WatersideBlock = 3m;

    public const decimal BurningDamage = 3m;

    public const decimal DarknessBlindStacks = 1m;

    public const decimal ForestRegenAmount = 3m;

    /// <summary>占最大生命值的比例，低于此值才获得再生。</summary>
    public const decimal ForestRegenHpRatio = 0.5m;

    public const decimal SunlightTurnStartVigor = 4m;

    public const decimal CityEnergy = 1m;

    /// <summary>只给没有「人工制品」的敌人。</summary>
    public const decimal CityArtifactStacks = 1m;

    public static async Task OnPlayerSideTurnStart(ICombatState combat)
    {
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
                    // Apply 是设总量：须读当前活力再加增量，直接传增量会覆盖原层数。
                    await PowerCmd.Apply<VigorPower>(choiceContext, ally, vigor + SunlightTurnStartVigor, null, null);
                }
        }

        // City 最后结算：人工制品会抵消之后施加的减益，不能抢在黑暗等场地之前。
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

    /// <remarks>必须挂 <c>AfterPlayerTurnStart</c>（每玩家各一次）而非 side 级：虚数空间按玩家独立进出。</remarks>
    public static Task OnPlayerTurnStart(Player player)
    {
        return FgoVoidHand.OnPlayerTurnStart(player);
    }

    private static List<Creature> LivingUnits(ICombatState combat)
    {
        return combat.Creatures.Where(static c => c.IsAlive).ToList();
    }

    private static List<Creature> LivingAllies(ICombatState combat)
    {
        return combat.Allies.Where(static c => c.IsAlive).ToList();
    }

    /// <summary>抵消 <c>PowerCmd.Apply</c> 对敌人的首次施加多人缩放，保证敌我层数一致。</summary>
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