using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class BlessedScion : FgoCardModel
{
    public BlessedScion() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Copies", 1, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await BlessedScionCmd.Execute(choiceContext, Owner.Creature, DynamicVars["Copies"].IntValue, this);
    }
}