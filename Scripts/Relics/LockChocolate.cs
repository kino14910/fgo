using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Relics;

public class LockChocolate : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        return player == Owner ? amount + 1m : amount;
    }

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<CursePower>(new BlockingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, null);
    }
}