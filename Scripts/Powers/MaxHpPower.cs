using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Cards.DynamicVars;

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

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var delta = Amount - _lastAmount; // 相对于上次的增量
        if (delta != 0)
        {
            var hpChange = delta * DynamicVars["HpPerStack"].BaseValue;
            _appliedHpBoost += hpChange;
            _lastAmount = Amount;
            Owner.SetMaxHpInternal(Owner.MaxHp + hpChange);
        }

        if (!_eventsSubscribed)
        {
            Owner.PowerIncreased += OnPowerIncreased;
            Owner.PowerDecreased += OnPowerDecreased;
            _eventsSubscribed = true;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (_eventsSubscribed)
        {
            Owner.PowerIncreased -= OnPowerIncreased;
            Owner.PowerDecreased -= OnPowerDecreased;
            _eventsSubscribed = false;
        }

        var newMax = Math.Max(1, Owner.MaxHp - _appliedHpBoost);
        Owner.SetMaxHpInternal(newMax);
        if (Owner.CurrentHp > newMax)
            Owner.SetCurrentHpInternal(newMax);

        _appliedHpBoost = 0;
        _lastAmount = 0;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
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
        return Task.CompletedTask;
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
}