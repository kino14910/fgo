using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class TameshiMono() : FgoCardModel(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("ExhaustCount", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["ExhaustCount"].UpgradeValueBy(2);
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