using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Relics;

public class CosmicMedallion : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner.Creature) return;
        if (result.TotalDamage <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, result.TotalDamage * 2,
            ValueProp.Unpowered, null);
    }
}