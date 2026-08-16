using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

public class RoadOfFallingFlowersPower : FgoPowerModel
{
    private bool _isHealing;
    public decimal HealBonus { get; set; } = 0.3m;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (_isHealing) return;
        if (delta <= 0) return;
        if (creature != Owner.Player?.Creature) return;

        var extra = delta * HealBonus;
        if (extra <= 0) return;

        _isHealing = true;
        try
        {
            await CreatureCmd.Heal(Owner.Player!.Creature, extra, false);
        }
        finally
        {
            _isHealing = false;
        }
    }
}