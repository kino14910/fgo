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

        await resources.UseCommandSpell();
        // 注意: 不在此处立即同步到 RunSavedData。
        // 令咒的保存语义: 使用后仅更新内存值，在战斗结束时（AfterCombatVictory）才同步到 RunSavedData。
        // 这样退出战斗中再继续会恢复到战前值，而打赢后下一场战斗保留战后值。

        switch (selected)
        {
            case RepairSpiritOrigin:
                await CreatureCmd.Heal(player.Creature, HealAmount);
                break;
            case ReleaseNoblePhantasm:
                await resources.ModifyNp(NpAmount, player);
                break;
        }

        return true;
    }
}