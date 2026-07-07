using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Commands;

public static class DragonCoreCmd
{
    public static async Task Execute(PlayerChoiceContext choiceContext, Creature owner, bool upgraded)
    {
        var player = owner.Player;
        if (player == null) return;
        var cards = player.PlayerCombatState!.Hand.Cards
            .Where(card => card.Type != CardType.Attack)
            .ToList();

        foreach (var card in cards)
            await CardCmd.Exhaust(choiceContext, card, true);

        var source = cards.FirstOrDefault() ?? player.Deck.Cards.FirstOrDefault();
        if (source == null) return;

        await FgoCardActions.AddRandomAttacksToHand(source, cards.Count, true, true);
    }
}