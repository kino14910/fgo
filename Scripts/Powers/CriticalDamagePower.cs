using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

/// <summary>
///     暴击威力 Power。
///     基础暴击：Stars >= 10 + 攻击牌 + 非宝具 → 消耗10星 → 200%伤害。
///     翡翠的魅力特殊：Stars >= 20 + 翡翠的魅力 → 消耗20星 → 300%伤害。
/// </summary>
public class CriticalDamagePower : FgoPowerModel
{
    private const decimal BasicCritMultiplier = 1m; // 200% = 1 + 1
    private const decimal SpecialCritMultiplier = 2m; // 300% = 1 + 2

    /// <summary>
    ///     暴击触发标记，供其他 Power（如 HeroicKingPower）读取。
    /// </summary>
    internal static bool CritTriggered;

    private bool _critActive;
    private decimal _critMultiplier;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        _critActive = false;
        _critMultiplier = 0m;

        if (cardPlay.Card is not { } card) return Task.CompletedTask;

        var resources = this.FgoRes();

        // 特殊暴击：翡翠的魅力，Stars >= 20
        if (card is CharismaOfTheJade && resources.TryConsumeCritStars(true) > 0)
        {
            _critActive = true;
            _critMultiplier = SpecialCritMultiplier;
        }
        // 基础暴击：攻击牌 + 非宝具，Stars >= 10
        else if (card is { Type: CardType.Attack } and not NobleCardModel
                 && resources.TryConsumeCritStars(false) > 0)
        {
            _critActive = true;
            _critMultiplier = BasicCritMultiplier;
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner != dealer) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (!_critActive) return 1m;

        CritTriggered = true;
        return 1m + _critMultiplier;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _critActive = false;
        CritTriggered = false;
        return Task.CompletedTask;
    }
}