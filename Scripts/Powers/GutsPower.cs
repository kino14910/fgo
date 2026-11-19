using Fgo.Scripts.Commands;
using Fgo.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

public class GutsPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool ShouldDie(Creature creature)
    {
        // BeforeDeath 无法阻止死亡，须走 ShouldDie + AfterPreventingDeath 复活，否则死亡流程照常执行。
        if (creature != Owner || Amount <= 0) return true;

        // 仍让死亡流程进入复活，是为让同场生效的非叠加毅力（times>0）接手。
        return creature.Powers.OfType<NonStackableGutsPower>()
            .Any(power => power.DynamicVars["times"].BaseValue > 0);
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || Amount <= 0) return;

        Flash();
        await ReviveCmd.Execute(creature, Amount);
        // 必须移除 this 而非 Remove<GutsPower>：后者按 p is GutsPower 匹配，会误删 NonStackableGutsPower。
        await PowerCmd.Remove(this);
        await NotifyGutsRelics(creature);
    }

    internal static async Task NotifyGutsRelics(Creature creature)
    {
        if (creature.Player?.GetRelic<PhoenixFeather>() is { } feather)
            await feather.OnGutsTriggered(creature);
    }
}