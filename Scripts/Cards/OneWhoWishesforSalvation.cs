using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Fgo.Scripts.Cards;

public class OneWhoWishesforSalvation() : FgoCardModel(1, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NpPerTurnPower>(10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NpPerTurnPower)].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpPerTurnPower)].BaseValue, Owner.Creature, this);
    }
}