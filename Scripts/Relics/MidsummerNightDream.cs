using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

public class MidsummerNightDream : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool TryModifyPowerAmountReceived(PowerModel power, Creature target,
        decimal amount, Creature? source, out decimal modifiedAmount)
    {
        if (target == Owner.Creature && power is CursePower && amount > 0)
        {
            Flash();
            modifiedAmount = 0m;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }
}