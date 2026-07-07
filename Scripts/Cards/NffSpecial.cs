using Fgo.Scripts.Cards.Colorless;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Cards;

public class NffSpecial : FgoCardModel
{
    public NffSpecial() : base(0, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithCards(2, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await FgoCardActions.AddToPile(ModelDb.Card<PoisonousDagger>().ToMutable(), PileType.Discard);
    }
}