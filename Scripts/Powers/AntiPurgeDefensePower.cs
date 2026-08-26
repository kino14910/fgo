using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class AntiPurgeDefensePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     Like Buffer
    ///     We use Late because other effects may reduce damage taken to 0 too, and it's more player-friendly for them to
    ///     trigger first so that this power doesn't have to decrement.
    /// </summary>
    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return amount;
        return 0m;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        await CreatureCmd.Heal(Owner, 5m);
        await PowerCmd.Decrement(this);
    }

    public override decimal GetScaledAmountForMultiplayer(ICombatState combatState, Creature? applier, decimal amount,
        Creature target, CardModel? cardSource)
    {
        return ((combatState.Players.Count - 1) * 2 + 1) * amount;
    }
}