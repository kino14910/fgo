using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Commands;

public static class BlessedScionCmd
{
    public static async Task Execute(PlayerChoiceContext choiceContext, Creature owner, int count, CardModel play)
    {
        var player = owner.Player;
        var selectable = player!.PlayerCombatState!.Hand.Cards.ToList();
        if (selectable.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("gameplay_ui", "COPY_CARDS"), 0,
            Math.Min(count, selectable.Count))
        {
            Cancelable = true,
            RequireManualConfirmation = true
        };
        var selected = await CardSelectCmd.FromHand(choiceContext, player, prefs, _ => true, play);
        await FgoCardActions.AddCopiesToHand(selected, true);
    }
}