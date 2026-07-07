using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     一次性暴击威力 Power，叠加次数。
///     每层提供 200% 暴击伤害，每次暴击消耗一层，层数归零后移除。
/// </summary>
public class CriticalDamageOncePower : CriticalDamagePower, IPowerExtraIconAmountLabelSpecsProvider
{
    private const decimal OnceCritMultiplier = 1m; // 200% = 1 + 1

    public int Amount2 { get; set; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/CriticalDamagePower.png",
        "res://Fgo/images/powers/big/CriticalDamagePower.png"
    );

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.TopLeft,
                Amount2.ToString())
        ];
    }

    public string GetSecondAmount()
    {
        return Amount2.ToString();
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource,
        CardPlay? cardPlay)
    {
        CritTriggered = true;
        return base.ModifyDamageMultiplicative(target, amount, props, dealer, cardSource, cardPlay) +
               Amount2 / 100m; // 200%
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? cardSource)
    {
        if (Owner != dealer) return;
        if (!props.IsPoweredAttack()) return;
        if (result.TotalDamage <= 0) return;
        Flash();
        await PowerCmd.Decrement(this);
    }
}