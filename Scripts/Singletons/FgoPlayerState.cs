using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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

    public event Action<int>? NpChanged;

    public async Task ModifyNp(int amount, Player? player = null)
    {
        var old = Np;
        Np += amount;
        if (Np == 99 && old < 99) Np = 100;

        // OverCharge 随 NP 值联动：首次达到 200 / 300 各获得 1 层 OverChargePower；
        // 反之跌破对应阈值时扣除对应层数（与宝具打出获得的层数共用同一上限 4）。
        await SyncOverchargeFromNp(player, old);

        // 在角色头顶显示 "+xxNP" 浮动文本（金色 D4AF37）。只在增加 NP 且有玩家上下文时显示。
        if (amount > 0)
            await FgoNpGainVfx.Spawn(player, amount);
    }

    private async Task SyncOverchargeFromNp(Player? player, int old)
    {
        if (player == null) return;

        var current = Np;

        // 向上跨过阈值：各 +1。
        var gained = 0;
        if (current >= 200 && old < 200) gained++;
        if (current >= 300 && old < 300) gained++;

        // 向下跌破阈值：各 -1（即 NP 减少到 300 以下 / 200 以下时收回去掉层数）。
        var lost = 0;
        if (old >= 300 && current < 300) lost++;
        if (old >= 200 && current < 200) lost++;

        var net = gained - lost;
        if (net == 0) return;

        var creature = player.Creature;
        var existing = creature.GetPower<OverchargePower>();
        var baseCount = existing?.Amount ?? 0;
        var delta = Math.Clamp(baseCount + net, 0, OverchargePower.MaxOverCharge) - baseCount;
        if (delta != 0)
            await PowerCmd.Apply<OverchargePower>(
                new BlockingPlayerChoiceContext(), creature, delta, creature, null);
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
            await ModifyNp(card.EnergyCost.GetResolved() * FgoReflectedSettings.BaseNpPerCost * multiplier,
                card.Owner);
        }
    }

    public async Task OnBeforeAttack(AttackCommand command)
    {
        _crit.Reset();

        var card = command.CardPlay?.Card ?? command.ModelSource as CardModel;
        if (card is not FgoCardModel fgo)
            return;

        if (fgo is CharismaOfTheJade)
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

    /// <summary>
    ///     仅用于给卡牌预览的 damage 变量提供一致判断：预览时 (<paramref name="isPreview" /> == true)
    ///     按"是否拥有足够暴击星"判断该攻击是否将触发暴击；否则按本次攻击实际触发的 CritActive 判断。
    ///     这样暴击威力（加法加伤 power）的预览显示与翻倍（倍率加伤）保持一致。
    /// </summary>
    public bool WillCritOnPlay(CardModel? cardSource, bool isPreview)
    {
        if (isPreview)
        {
            if (cardSource is CharismaOfTheJade) return CanSpecialCrit;
            if (cardSource is { Type: CardType.Attack } and not NobleCardModel) return CanCrit;
            return false;
        }

        return _crit.Active;
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