using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BridalSpinWheel : NobleCardModel
{
    public BridalSpinWheel() : base(1, CardType.Skill, TargetType.Self)
    {
        WithPower<PlatingPower>(2, 1);
        WithStar(8, 4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(PlatingPower).Name].BaseValue, Owner.Creature, this);
        await FgoStarCmd.AddStars(DynamicVars["Star"].IntValue);
    }
}