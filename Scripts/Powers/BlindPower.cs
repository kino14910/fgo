using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

/// <summary>
///     〔盲目〕进行攻击时伤害降低 25%，每次攻击后移除 1 层。
/// </summary>
public class BlindPower : FgoPowerModel
{
    private const decimal DamageMultiplier = 0.75m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return DamageMultiplier;
    }

    /// <summary>
    ///     一次攻击结算完毕后扣 1 层。
    ///     <para>
    ///         刻意放在攻击级钩子而不是 <c>AfterDamageGiven</c>（伤害实例级): 后者对多段攻击会按段数
    ///         扣层，与「进行攻击就移除 1 层」不符。
    ///     </para>
    /// </summary>
    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker != Owner) return;

        Flash();
        await PowerCmd.Decrement(this);
    }
}