using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless.OptionCards;

public class Kaleidoscope : FgoColorlessCard
{
    public Kaleidoscope() : base(-1, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithNp(100);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}