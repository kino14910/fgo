using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(DeprecatedRelicPool))]
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