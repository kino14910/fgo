using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class SwordOfSelection : FgoCardModel
{
    public SwordOfSelection() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithCards(2, 1);
        WithNp(20);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await FgoNpCmd.AddNp(FgoCardActions.HandSize(Owner) * DynamicVars["NP"].IntValue / 10);
    }
}