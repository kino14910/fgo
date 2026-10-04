using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Cards;

public class SparkRoute() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        if (hand.Count == 0) return;

        var toDiscard = (IReadOnlyList<CardModel>)hand;

        if (IsUpgraded)
        {
            // 「任意数量」= 0 ~ 手牌数都合法，由玩家自选。
            var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, hand.Count);
            toDiscard = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).ToList();

            if (toDiscard.Count == 0) return;
        }

        await CardCmd.DiscardAndDraw(choiceContext, toDiscard, toDiscard.Count);
    }
}
