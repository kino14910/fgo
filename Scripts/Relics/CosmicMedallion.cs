using MegaCrit.Sts2.Core.Combat;
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
        if (!CombatManager.Instance.IsInProgress) return;
        if (Owner.Creature.CombatState?.CurrentSide != CombatSide.Player) return;
        if (target != Owner.Creature) return;
        if (result.UnblockedDamage <= 0) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, result.TotalDamage * 2,
            ValueProp.Unpowered, null);
    }
}