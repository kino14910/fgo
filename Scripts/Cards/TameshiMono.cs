using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class TameshiMono() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("ExhaustCount", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["ExhaustCount"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt,
            0, DynamicVars["ExhaustCount"].IntValue)
        {
            RequireManualConfirmation = true
        };
        var cards = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            PileType.Discard.GetPile(Owner).Cards,
            Owner,
            prefs)).ToList();

        foreach (var card in cards)
            await CardCmd.Exhaust(choiceContext, card);

        await FgoResCmd.ModifyStars(cards.Count * 4, play.Player);
    }
}