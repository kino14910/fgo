using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        if (handCount == 0) return;

        var prefs = new CardSelectorPrefs(SelectionScreenPrompt, 0, handCount);
        var selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).ToArray();

        if (selected.Length > 0) await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
    }
}