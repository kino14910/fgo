using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CalamityOfTheNorth() : FgoCardModel(2, CardType.Skill,
    CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<PoisonPower>(4),
        ModCardVars.Power<CursePower>(4)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PoisonPower)].UpgradeValueBy(2);
        DynamicVars[nameof(CursePower)].UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<PoisonPower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(PoisonPower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<CursePower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(CursePower)].BaseValue,
            Owner.Creature, this);
    }
}