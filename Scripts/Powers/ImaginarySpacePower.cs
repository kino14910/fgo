using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

/// <summary>
///     〔虚数空间〕本身：挂在玩家 <c>Creature</c> 上的隐藏状态标记，不计时。
/// </summary>
/// <remarks>
///     「能力在身」等价于「该玩家在虚数空间内」，由 <c>FgoVoidHand</c> 施加/解除并据此替换手牌；
///     没有倒计时，只有打出〔虚数脱出〕走的 <c>FgoVoidHand.Exit</c> 才会移除它。
///     联机时每名玩家各持一份，互不干扰；两个钩子都按持有者过滤，不能拦住别人的弃牌/抽牌。
///     不抽牌/不弃牌只在 <c>FgoVoidHand.IsVoidHandActive</c> 为真时生效；任何方式退出 voidHand 后都必须立即失效。
/// </remarks>
public class ImaginarySpacePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override bool ShouldFlush(Player player)
    {
        return player != Owner.Player || !FgoVoidHand.IsVoidHandActive(player);
    }

    /// <summary>回合开始那次抽牌带 <c>fromHandDraw: true</c>，放行它就挡不住固定抽牌。</summary>
    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        return player != Owner.Player || !FgoVoidHand.IsVoidHandActive(player);
    }
}
