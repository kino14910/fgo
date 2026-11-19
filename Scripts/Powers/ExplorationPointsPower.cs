using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

/// <summary>
///     探索点数: 在〔虚数空间〕中累积的资源。由 <c>VoidEscape</c>（虚数脱出）一次性消耗，
///     每 1 点永久提升 <c>GreatVoidSeaBattle</c>（虚数大海战）获得的宝具值。
/// </summary>
/// <remarks>
/// </remarks>
public class ExplorationPointsPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public static async Task<int> TakeAll(Creature creature)
    {
        var points = creature.GetPower<ExplorationPointsPower>()?.Amount ?? 0;
        if (points > 0) await PowerCmd.Remove<ExplorationPointsPower>(creature);
        return points;
    }
}