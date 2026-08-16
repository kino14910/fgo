using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class MaxHpPower : FgoPowerModel
{
    private decimal _appliedHpBoost;
    private bool _eventsSubscribed;
    private int _lastAmount;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("HpPerStack", 1m)
    ];

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _appliedHpBoost = 0;
        _lastAmount = 0;
        _eventsSubscribed = false;
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _lastAmount = Amount;
        _appliedHpBoost = _lastAmount * DynamicVars["HpPerStack"].BaseValue;
        ApplyMaxHpBoost(_appliedHpBoost);

        if (!_eventsSubscribed)
        {
            Owner.PowerIncreased += OnPowerIncreased;
            Owner.PowerDecreased += OnPowerDecreased;
            _eventsSubscribed = true;
        }

        await Task.CompletedTask;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (_eventsSubscribed)
        {
            oldOwner.PowerIncreased -= OnPowerIncreased;
            oldOwner.PowerDecreased -= OnPowerDecreased;
            _eventsSubscribed = false;
        }

        var newMax = Math.Max(1, oldOwner.MaxHp - _appliedHpBoost);
        oldOwner.SetMaxHpInternal(newMax);
        if (oldOwner.CurrentHp > newMax)
            oldOwner.SetCurrentHpInternal(newMax);

        _appliedHpBoost = 0;
        _lastAmount = 0;
        await Task.CompletedTask;
    }

    private void OnPowerIncreased(PowerModel power, int change, bool silent)
    {
        if (power != this) return;
        SyncAfterAmountChange();
    }

    private void OnPowerDecreased(PowerModel power, bool silent)
    {
        if (power != this) return;
        SyncAfterAmountChange();
    }

    private void SyncAfterAmountChange()
    {
        var delta = Amount - _lastAmount;
        if (delta == 0) return;

        var hpChange = delta * DynamicVars["HpPerStack"].BaseValue;
        _appliedHpBoost += hpChange;
        _lastAmount = Amount;

        Owner.SetMaxHpInternal(Owner.MaxHp + hpChange);
        if (Owner.CurrentHp > Owner.MaxHp)
            Owner.SetCurrentHpInternal(Owner.MaxHp);
    }

    private void ApplyMaxHpBoost(decimal amount)
    {
        Owner.SetMaxHpInternal(Owner.MaxHp + amount);
        // 不恢复CurrentHp
    }
}