using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Fgo.Scripts.Cards;

public class Indomitable() : FgoCardModel(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NonStackableGutsPower>(3),
        new PowerVar<IndomitablePower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NonStackableGutsPower)].UpgradeValueBy(2);
        DynamicVars[nameof(IndomitablePower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NonStackableGutsPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<IndomitablePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(IndomitablePower)].BaseValue, Owner.Creature, this);
    }
}