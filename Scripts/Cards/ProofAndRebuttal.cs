using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ProofAndRebuttal() : FgoCardModel(0, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
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