using MegaCrit.Sts2.Core.Entities.Players;

namespace Fgo.Scripts.Utils;

/// <summary>
///     单场战斗的〔虚数空间〕参与状态: 记录哪些玩家已经完成过「原手牌 → 额外手牌」的替换。
/// </summary>
/// <remarks>
///     <para>
///         之所以要显式记账，而不能靠「暂存牌堆空不空」反推: <c>VoidHold</c> 为空既可能是
///         「已经退出」，也可能是「进入时手牌本来就是空的」。这两种情况对
///         <see cref="FgoVoidHand" /> 的处理完全不同（后者仍需补齐额外手牌、
///         仍算身处虚数空间），靠牌堆内容猜会把「空手进入」的玩家误判成「已退出」，
///         于是他既拿不到〔虚数脱出〕、又拿不回手牌，成为死锁。
///     </para>
///     <para>
///         <b>跨端一致性</b>: 写入只发生在被复制的游戏动作内（出牌 <c>OnPlay</c>、回合钩子），
///         各端各跑一遍同一份逻辑，因此不需要额外的 Sidecar 广播。
///         <see cref="Members" /> 只做成员判定，<b>绝不用于迭代</b>——<see cref="HashSet{T}" /> 的
///         枚举顺序不保证跨端一致；所有遍历一律走 <c>ICombatState.Players</c> 的确定顺序。
///     </para>
/// </remarks>
public sealed class FgoVoidSpaceState
{
    private readonly HashSet<Player> _members = [];

    /// <summary>
    ///     本场战斗是否正处在〔虚数空间〕中（= 至少有一名玩家在场）。
    ///     「自然到期」的判定基准，见 <see cref="FgoVoidHand.ExitIfInactive" />。
    /// </summary>
    public bool Active { get; private set; }

    /// <summary>累计真正进入过几次（重复打出同一张卡不会重复计数）。</summary>
    public int SessionCount { get; private set; }

    /// <summary>该玩家是否已完成手牌替换、正在虚数空间里。</summary>
    public bool IsMember(Player player) => _members.Contains(player);

    /// <summary>
    ///     标记玩家进入。返回 true 表示这次是「本场战斗的首次进入」，
    ///     调用方据此保证重复施加不会重复计入次数。
    /// </summary>
    public bool Join(Player player)
    {
        var firstTime = _members.Add(player);
        if (Active) return false;

        Active = true;
        SessionCount++;
        return firstTime;
    }

    public void Leave(Player player) => _members.Remove(player);

    /// <summary>退出全体：清空成员名单并结束本场次（战斗结束或场地被清时也会走到这里）。</summary>
    public void Reset()
    {
        _members.Clear();
        Active = false;
    }
}
