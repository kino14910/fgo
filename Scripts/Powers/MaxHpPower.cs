using System;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;
public class MaxHpPower : ModPowerTemplate
{
    private decimal _appliedHpBoost;
    private int _lastAmount;
    private bool _eventsSubscribed;

    protected override IEnumerable<DynamicVar> CanonicalVars => new[]
    {
        new DynamicVar("HpPerStack", 10m),  // 在这里配置每层HP
    };

    private decimal HpPerStack => DynamicVars["HpPerStack"].BaseValue;

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
        _appliedHpBoost = _lastAmount * HpPerStack;
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

        decimal newMax = Math.Max(1, oldOwner.MaxHp - _appliedHpBoost);
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
        int delta = Amount - _lastAmount;
        if (delta == 0) return;

        decimal hpChange = delta * HpPerStack;
        _appliedHpBoost += hpChange;
        _lastAmount = Amount;

        Owner.SetMaxHpInternal(Owner.MaxHp + hpChange);
        if (Owner.CurrentHp > Owner.MaxHp)
            Owner.SetCurrentHpInternal(Owner.MaxHp);
    }

    private void ApplyMaxHpBoost(decimal amount)
    {
        Owner.SetMaxHpInternal(Owner.MaxHp + amount);
        // 不恢复CurrentHp — 这正是你要的效果
    }
}