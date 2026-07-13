using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class CalamityOfTheNorth() : FgoCardModel(2, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(5),
        new PowerVar<CursePower>(5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PoisonPower)].UpgradeValueBy(3);
        DynamicVars[nameof(CursePower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, DynamicVars[nameof(PoisonPower)].BaseValue,
                Owner.Creature, this);
            await PowerCmd.Apply<CursePower>(choiceContext, enemy, DynamicVars[nameof(CursePower)].BaseValue,
                Owner.Creature, this);
        }
    }
}