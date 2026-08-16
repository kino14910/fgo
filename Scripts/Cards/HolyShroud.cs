using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HolyShroud() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedPowerAmountGiven<ReducePercentDamagePower>(
            20,
            (card, _) =>
            {
                var combatState = card?.CombatState;
                if (combatState == null)
                    return 0m;

                return combatState.Enemies
                    .Where(e => e.IsAlive)
                    .Sum(enemy =>
                    {
                        if (enemy.Monster is not { } monster)
                            return 0m;

                        var move = monster.NextMove;
                        return move.Intents
                            .Where(intent => intent.IntentType == IntentType.Attack
                                             || intent.IntentType == IntentType.DeathBlow)
                            .OfType<AttackIntent>()
                            .Sum(intent => (decimal)intent.GetTotalDamage(
                                combatState.PlayerCreatures,
                                enemy));
                    });
            })
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}