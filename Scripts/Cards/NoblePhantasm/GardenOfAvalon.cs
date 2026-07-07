using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class GardenOfAvalon : NobleCardModel
{
    public GardenOfAvalon() : base(3, CardType.Power, TargetType.Self)
    {
        WithBlock(3, 2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature, 10m, Owner.Creature, this);
        await PowerCmd.Apply<StarsPerTurnPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
    }
}