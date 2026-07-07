using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Unlimited : NobleCardModel
{
    public Unlimited() : base(1, CardType.Power, TargetType.Self)
    {
        WithVar("AttacksPerTurn", 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<UnlimitedPower>(choiceContext, Owner.Creature, DynamicVars["AttacksPerTurn"].BaseValue,
            Owner.Creature, this);
    }
}