using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace Fgo.Scripts.Cards.Colorless;

public class ProofAndRebuttal : FgoColorlessCard
{
    public ProofAndRebuttal() : base(1, CardType.Skill,
        CardRarity.Token, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, 1m, Owner);
        var handCount = Owner.PlayerCombatState!.Hand.Cards.Count;
        if (handCount == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "TOP_DECK_CARDS"), 0, handCount)
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, _ => true, this)).ToList();
        foreach (var card in selected)
            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top, this, true);
    }
}