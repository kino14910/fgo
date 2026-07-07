using Fgo.Scripts.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Events;

[RegisterSharedEvent]
public class DevilSlot : FgoEventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return
        [
            new EventOption(this, BB, InitialOptionKey("BB")).WithRelic<BB>(Owner!),
            new EventOption(this, RemoveCard, InitialOptionKey("REMOVE"))
        ];
    }

    public async Task BB()
    {
        var relic = ModelDb.Relic<BB>().ToMutable();
        await RelicCmd.Obtain(relic, Owner!);
        SetEventFinished(PageDescription("BB"));
    }

    public async Task RemoveCard()
    {
        // 从主牌组中选择一张卡牌移除（非基础、非诅咒卡牌）
        var removableCards = Owner!.Deck.Cards
            .Where(c => c.Rarity != CardRarity.Basic && c.Type != CardType.Curse)
            .ToList();

        if (removableCards.Count > 0)
        {
            var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "DevilSlot.text_remove"), 1)
            {
                RequireManualConfirmation = true
            };
            var selected = (await CardSelectCmd.FromSimpleGrid(
                new BlockingPlayerChoiceContext(), removableCards, Owner!, prefs)).FirstOrDefault();

            if (selected != null) await CardPileCmd.RemoveFromDeck(selected);
        }

        SetEventFinished(PageDescription("REMOVE"));
    }
}