using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.HealthBars;

namespace Fgo.Scripts.Powers;

/// <summary>
///     诅呪: 不衰减的中毒。每个单位在自己一方回合开始时失去等同于层数的生命。
/// </summary>
public class CursePower : FgoPowerModel, IHealthBarForecastSource
{
    /// <summary>生命条预测色（诅呪为紫色，区别于中毒的绿色）。</summary>
    private static readonly Color ForecastColor = new("C77DFF");

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), Owner, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    }

    /// <summary>
    ///     生命条上「本回合将因诅呪失去的生命」。与原版中毒一致地过一遍
    ///     <see cref="Hook.ModifyDamage" />，使虚弱/易伤等修正体现在预测里。
    /// </summary>
    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        if (context.Creature.CombatState is not { } combatState) return [];
        if (Amount <= 0) return [];

        var damage = Hook.ModifyDamage(combatState.RunState, combatState, context.Creature, null, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null, ModifyDamageHookType.All,
            CardPreviewMode.None, out _);
        if (damage <= 0m) return [];

        var order = HealthBarForecastOrder.ForSideTurnStart(context.Creature, context.Creature.Side);
        return HealthBarForecasts.Single((int)damage, ForecastColor,
            HealthBarForecastGrowthDirection.FromRight, order);
    }
}
