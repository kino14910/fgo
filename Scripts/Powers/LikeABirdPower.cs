using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     〔好似飞鸟〕造成的伤害变为 200%。
/// </summary>
/// <remarks>
///     由 <c>LikeABird</c>（好似飞鸟）卡打出时施加，玩家离开〔虚数空间〕时由 <c>FgoVoidHand</c> 移除；
///     因此「能力在身」本身就等价于「人在虚数空间」，这里不必再查场地。
/// </remarks>
public class LikeABirdPower : FgoPowerModel
{
    /// <summary>伤害倍率: 200%。</summary>
    private const decimal DamageMultiplier = 2m;

    public override PowerType Type => PowerType.Buff;

    /// <summary>层数无意义（效果是开关），用 <see cref="PowerStackType.Single" /> 隐藏角标。</summary>
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AtkUpPower.png",
        "res://Fgo/images/powers/big/AtkUpPower.png"
    );
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return DamageMultiplier;
    }
}
