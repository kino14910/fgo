using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class MonteCristoTreasure : FgoCardModel
{
    public MonteCristoTreasure() : base(3, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithVar("Multiplier", 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<MonteCristoTreasurePower>(choiceContext, Owner.Creature,
            DynamicVars["Multiplier"].BaseValue, Owner.Creature, this);
    }
}