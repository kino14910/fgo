using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class GoddessMetamorphosisBeast : FgoCardModel
{
    public GoddessMetamorphosisBeast() : base(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<PoisonPower>(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<GoddessMetamorphosisBeastPower>(choiceContext, Owner.Creature,
            DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
    }
}