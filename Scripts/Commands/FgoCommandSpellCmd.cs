using Fgo.Scripts.Cards.Colorless.OptionCards;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Commands;

public static class FgoCommandSpellCmd
{
    private const int HealAmount = 30;
    private const int NpAmount = 100;

    public static async Task<bool> TryUseCommandSpell(PlayerChoiceContext choiceContext, Player player)
    {
        var resources = ModelDb.Singleton<FgoPlayerResources>();
        if (!resources.CanUseCommandSpell) return false;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return false;
        var cards = new List<CardModel>
        {
            combatState.CreateCard<RepairSpiritOrigin>(player),
            combatState.CreateCard<ReleaseNoblePhantasm>(player)
        };

        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards, player);
        if (selected == null) return false;

        resources.UseCommandSpell();

        switch (selected)
        {
            case RepairSpiritOrigin:
                await CreatureCmd.Heal(player.Creature, HealAmount);
                break;
            case ReleaseNoblePhantasm:
                await resources.AddNp(NpAmount, choiceContext, player);
                break;
        }

        return true;
    }
}