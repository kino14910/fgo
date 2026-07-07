using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Powers;

public class EvasionPower : FgoPowerModel
{
    private bool _blocked;
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 闪避：对移动类伤害（攻击）完全免疫
        if (target == Owner && amount > 0 && props.HasFlag(ValueProp.Move))
        {
            _blocked = true;
            return 0m;
        }

        return 1m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // ModifyDamageMultiplicative 将伤害归零后，BeforeDamageReceived 收到的 amount 已是 0，
        // 无法据此判断是否闪避。改用标志在 AfterDamageReceived 中扣层。
        if (target == Owner && _blocked)
        {
            _blocked = false;
            Flash();
            await PowerCmd.Decrement(this);
        }
    }
}