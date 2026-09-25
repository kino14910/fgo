using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Powers;

public class RoadOfFallingFlowersPower : FgoPowerModel
{
    private bool _isHealing;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Int("HealPercent", 30)];

    public decimal HealPercent
    {
        get => DynamicVars["HealPercent"].BaseValue;
        set
        {
            DynamicVars["HealPercent"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    private decimal HealBonus => HealPercent / 100m;

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