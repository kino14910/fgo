using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HolyShroud() : FgoCardModel(0, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];

    protected override bool ShouldGlowGoldInternal
    {
        get
        {
            var incomingDamage = CombatState?.Enemies
                .Where(e => e.IsAlive)
                .Sum(enemy =>
                {
                    if (enemy.Monster is not { } monster)
                        return 0m;

                    return monster.NextMove.Intents
                        .Where(intent =>
                            intent.IntentType is IntentType.Attack or IntentType.DeathBlow)
                        .OfType<AttackIntent>()
                        .Sum(intent =>
                            (decimal)intent.GetTotalDamage(
                                CombatState.PlayerCreatures,
                                enemy));
                });

            return incomingDamage >= 10;
        }
    }

    protected override bool ShouldGlowRedInternal => !ShouldGlowGoldInternal;

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ShouldGlowGoldInternal)
            await PowerCmd.Apply<ReducePercentDamagePower>(
                choiceContext,
                Owner.Creature,
                DynamicVars[nameof(ReducePercentDamagePower)].BaseValue,
                Owner.Creature,
                this);
    }
}