using Fgo.Scripts.Commands;
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
        // BeforeDeath 无法阻止死亡，须像官方 FairyInABottle/LizardTail 一样
        // 通过 ShouldDie + AfterPreventingDeath 实现复活，否则死亡流程照常执行
        // （卡牌耗尽、血条 UI 消失）而角色血量已被恢复。
        if (creature != Owner || Amount <= 0) return true;

        // 存在可用的不可叠加毅力时优先由其处理，保证与 FgoPlayerState.ShouldDie 的
        // 判定优先级一致（先 NonStackableGutsPower，再普通 GutsPower）。
        if (creature.Powers.OfType<NonStackableGutsPower>()
            .Any(power => power.DynamicVars["times"].BaseValue > 0))
            return true;

        return false;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || Amount <= 0) return;

        Flash();
        await ReviveCmd.Execute(creature, Amount);
        // 必须移除 this 而非 Remove<GutsPower>：
        // GetPower<GutsPower> 匹配的是 p is GutsPower，会误删 NonStackableGutsPower 实例。
        await PowerCmd.Remove(this);
    }
}