using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

/// <summary>
///     探索点数: 在〔虚数空间〕中累积的资源。由 <c>VoidEscape</c>（虚数脱出）一次性消耗，
///     每 1 点永久提升 <c>GreatVoidSeaBattle</c>（虚数大海战）获得的宝具值。
/// </summary>
/// <remarks>
///     做成 Power 而不是局内存档字段: 层数即点数，天然带角标显示、悬停提示与战斗级生命周期。
///     玩家离开〔虚数空间〕时必然经 <c>VoidEscape</c> 清空，所以"战斗结束即失效"符合设计。
///     <para />
///     不加静态包装: <b>增加</b>直接写
///     <c>PowerCmd.Apply&lt;ExplorationPointsPower&gt;(ctx, creature, +N, creature, cardSource)</c>，
///     <b>读取</b>直接写 <c>creature.GetPower&lt;ExplorationPointsPower&gt;()?.Amount ?? 0</c>。
///     唯一保留的助手是 <see cref="TakeAll" />——「读 + 清空 + 返回旧值」是个原子语义，展开到调用点要三行，
///     而 <c>Gain</c> / <c>Of</c> 那类只省类型参数或只做一次空兜底的转发没有存在价值。
/// </remarks>
public class ExplorationPointsPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>取走并清空全部探索点数，返回被取走的值；没有该能力时为 0。</summary>
    public static async Task<int> TakeAll(Creature creature)
    {
        var points = creature.GetPower<ExplorationPointsPower>()?.Amount ?? 0;
        if (points > 0) await PowerCmd.Remove<ExplorationPointsPower>(creature);
        return points;
    }
}
