using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Singletons;

/// <summary>
///     单个 FGO 玩家的战斗资源状态（NP、暴击星、令咒、OverCharge、暴击上下文）。
///     每个玩家独立一份，取代原先的全局单例字段，避免多人模式下串台。
///     通过 <see cref="FgoBattleHooks.Get(MegaCrit.Sts2.Core.Entities.Players.Player)" /> 按 Player 索引。
/// </summary>
public sealed class FgoPlayerState
{
    private const int MaxCommandSpell = 3;
    private const int MaxOverCharge = 4;
    private const int BasicCritStars = 10;
    private const int SpecialCritStars = 20;

    private readonly CritContext _crit = new();

    private int _commandSpell = 3;
    private int _mashUpgradeLevel;
    private int _np;
    private bool _npButtonPressed;
    private int _stars;

    public bool CritTriggered => _crit.Triggered;
    public bool CritActive => _crit.Active;
    public decimal CritDamageMultiplier => _crit.DamageMultiplier;

    public int Np
    {
        get => _np;
        private set
        {
            var clamped = Math.Clamp(value, 0, 300);
            if (_np == clamped) return;
            _np = clamped;
            NpChanged?.Invoke(_np);
        }
    }

    public int Stars
    {
        get => _stars;
        private set => _stars = Math.Max(0, value);
    }

    public int CommandSpell
    {
        get => _commandSpell;
        private set => _commandSpell = Math.Clamp(value, 0, MaxCommandSpell);
    }

    public int MashUpgradeLevel
    {
        get => _mashUpgradeLevel;
        set => _mashUpgradeLevel = Math.Max(0, value);
    }

    public bool CanCrit => Stars >= BasicCritStars;
    public bool CanSpecialCrit => Stars >= SpecialCritStars;
    public bool CanUseNp => Np >= 100;
    public bool CanUseCommandSpell => CommandSpell > 0;
    public int OverCharge { get; private set; }
    public int OverChargeLevel => OverCharge;

    public event Action<int>? NpChanged;

    public async Task ModifyNp(int amount, Player? player = null)
    {
        var old = Np;
        Np += amount;
        if (Np == 99 && old < 99) Np = 100;
        await SyncOverCharge(old, Np);

        // 在角色头顶显示 "+xxNP" 浮动文本（金色 D4AF37）。只在增加 NP 且有玩家上下文时显示。
        if (amount > 0)
            await FgoNpGainVfx.Spawn(player, amount);
    }

    public Task GainOverCharge(int amount)
    {
        if (amount <= 0) return Task.CompletedTask;
        OverCharge = Math.Clamp(OverCharge + amount, 0, MaxOverCharge);
        return Task.CompletedTask;
    }

    public Task SpendNpForNoblePhantasm()
    {
        Np = 0;
        return Task.CompletedTask;
    }

    public async Task ModifyStars(int amount, Player? player = null)
    {
        Stars += amount;

        if (amount > 0)
            await FgoStarGainVfx.Spawn(player, amount);
    }

    public Task ResetStars()
    {
        Stars = 0;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     尝试消耗暴击星。返回实际消耗的数量（0=未触发）。
    /// </summary>
    public async Task<int> TryConsumeCritStars(bool special)
    {
        var required = special ? SpecialCritStars : BasicCritStars;
        if (Stars < required) return 0;
        await ModifyStars(-required);
        return required;
    }

    public Task<bool> UseCommandSpell(int amount = 1)
    {
        if (CommandSpell < amount) return Task.FromResult(false);
        CommandSpell -= amount;
        return Task.FromResult(true);
    }

    public Task LoadCommandSpellFromRunState(Player player)
    {
        var saved = Entry.RunState.Get(player);
        _commandSpell = Math.Clamp(saved.CommandSpellCount, 0, MaxCommandSpell);
        return Task.CompletedTask;
    }

    public Task SaveCommandSpellToRunState(Player player)
    {
        Entry.RunState.Modify(player, data => { data.CommandSpellCount = _commandSpell; });
        return Task.CompletedTask;
    }

    public Task ResetCommandSpell()
    {
        CommandSpell = MaxCommandSpell;
        return Task.CompletedTask;
    }

    public Task Reset()
    {
        Np = 0;
        _stars = 0;
        OverCharge = 0;
        _npButtonPressed = false;
        return Task.CompletedTask;
    }

    public Task SetNpButtonPressed()
    {
        _npButtonPressed = true;
        return Task.CompletedTask;
    }

    public async Task OnBeforeCardPlayed(CardPlay cardPlay)
    {
        _crit.Reset();

        if (cardPlay.Card is not { } card)
            return;

        if (card is FgoCardModel)
        {
            var multiplier = card.Owner.Creature.HasPower<NpRatePower>() ? 2 : 1;
            await ModifyNp(card.EnergyCost.Canonical * FgoReflectedSettings.BaseNpPerCost * multiplier,
                card.Owner);

            if (card is CharismaOfTheJade)
            {
                if (await TryConsumeCritStars(true) > 0)
                {
                    _crit.Active = true;
                    _crit.DamageMultiplier = 3m;
                }
            }
            else if (card is { Type: CardType.Attack } and not NobleCardModel)
            {
                if (await TryConsumeCritStars(false) > 0)
                {
                    _crit.Active = true;
                    _crit.DamageMultiplier = 2m;
                }
            }
        }
    }

    public decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer?.Player == null || !props.IsPoweredAttack())
            return 1m;

        if (cardPlay == null)
        {
            if (cardSource is CharismaOfTheJade) return CanSpecialCrit ? 3m : 1m;
            if (cardSource is { Type: CardType.Attack } and not NobleCardModel) return CanCrit ? 2m : 1m;
            return 1m;
        }

        if (!_crit.Active)
            return 1m;

        _crit.Triggered = true;
        return _crit.DamageMultiplier;
    }

    public void ResetCrit()
    {
        _crit.Reset();
    }

    public void OnAfterPlayerTurnStart()
    {
        if (_npButtonPressed && CanUseNp) _npButtonPressed = false;
    }

    public bool ShouldDie(Creature creature)
    {
        if (!(creature is { IsPlayer: true, Player.Character: FgoCharacter }
              && CommandSpell >= 3))
            return true;

        // 毅力可用性: 优先 NonStackableGutsPower（需剩余次数 > 0），其次普通 GutsPower（存在即可用）。
        var hasUsableGuts =
            creature.Powers.OfType<NonStackableGutsPower>().Any(g => g.DynamicVars["times"].BaseValue > 0)
            || creature.Powers.OfType<GutsPower>().Any(g => g is not NonStackableGutsPower);

        return hasUsableGuts;
    }

    public async Task AfterPreventingDeath(Creature creature)
    {
        if (CommandSpell < 3)
            return;

        CommandSpell -= 3;
        await CreatureCmd.Heal(creature, creature.MaxHp - creature.CurrentHp);
        if (Np < 300)
            await ModifyNp(300 - Np, creature.Player);
    }

    public void OnAfterSideTurnEnd(CombatSide side)
    {
        if (side == CombatSide.Player)
            OverCharge = 0;
    }

    private static int OverChargeLevelFor(int np)
    {
        return np >= 300 ? 2 : np >= 200 ? 1 : 0;
    }

    private async Task SyncOverCharge(int oldNp, int newNp)
    {
        var delta = OverChargeLevelFor(newNp) - OverChargeLevelFor(oldNp);
        switch (delta)
        {
            case > 0:
                await GainOverCharge(delta);
                break;
            case < 0:
                OverCharge = Math.Max(0, OverCharge + delta);
                break;
        }
    }

    private sealed class CritContext
    {
        public bool Active;
        public decimal DamageMultiplier;
        public bool Triggered;

        public void Reset()
        {
            Active = false;
            Triggered = false;
            DamageMultiplier = 1m;
        }
    }
}