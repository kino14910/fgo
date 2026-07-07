using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class TameshiMono : FgoCardModel
{
    public TameshiMono() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithVar("ExhaustCount", 3, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var cards = Owner.PlayerCombatState!.DiscardPile.Cards
            .Take(DynamicVars["ExhaustCount"].IntValue)
            .ToList();

        foreach (var card in cards)
            await CardCmd.Exhaust(choiceContext, card, true);

        await FgoStarCmd.AddStars(cards.Count * 4);
    }
}