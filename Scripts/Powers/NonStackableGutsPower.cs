using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

/// <summary>
///     不叠加的毅力：每次施加独立存在（各占一个图标），死亡时消耗一层并按该层的回血量恢复。
///     不同回血量的实例互不合并，各自显示独立的图标与数值。
/// </summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
public class NonStackableGutsPower : GutsPower
{
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>
    ///     基类 GutsPower 在 BeforeDeath 中治疗并移除自身，但 BeforeDeath 无法阻止死亡。
    ///     这里改为通过 ShouldDie + AfterPreventingDeath 实现真正的复活。
    /// </summary>
    public override Task BeforeDeath(Creature creature)
    {
        return Task.CompletedTask;
    }

    public override bool ShouldDie(Creature creature)
    {
        // 仅当自身所属生物即将死亡、且本层仍有回血量时阻止死亡
        return creature != Owner || Amount <= 0;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner || Amount <= 0)
            return;

        Flash();
        await CreatureCmd.Heal(creature, Amount);
        await PowerCmd.Remove(this);
    }

    [HarmonyPatch]
    private class OldInstancedPatch
    {
        private static readonly MethodInfo?
            TargetMethod = AccessTools.PropertyGetter(typeof(PowerModel), "IsInstanced");

        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (TargetMethod != null) yield return TargetMethod;
        }

        private static bool Prepare()
        {
            return TargetMethod != null;
        }

        [HarmonyPrefix]
        private static bool MakeInstanced(PowerModel __instance, ref bool? __result)
        {
            if (__instance is not NonStackableGutsPower) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    private class NewInstancedPatch
    {
        private static readonly MethodInfo? TargetMethod =
            AccessTools.PropertyGetter(typeof(PowerModel), "InstanceType");

        private static readonly Type? InstanceTypeEnum =
            typeof(PowerModel).Assembly.GetType("MegaCrit.Sts2.Core.Entities.Powers.PowerInstanceType");

        private static IEnumerable<MethodBase> TargetMethods()
        {
            if (TargetMethod != null) yield return TargetMethod;
        }

        private static bool Prepare()
        {
            return TargetMethod != null;
        }

        [HarmonyPrefix]
        private static bool MakeInstanced(PowerModel __instance, ref object? __result)
        {
            if (__instance is not NonStackableGutsPower) return true;

            if (InstanceTypeEnum == null)
                throw new InvalidOperationException("Could not get PowerInstanceType enum type");

            __result = InstanceTypeEnum.GetEnumValues().GetValue(1);
            return false;
        }
    }
}