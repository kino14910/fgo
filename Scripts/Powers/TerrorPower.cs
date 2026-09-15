using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class TerrorPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnDebuffPower.png",
        "res://Fgo/images/powers/big/EveryTurnDebuffPower.png"
    );

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Stun)
    ];

    /// <summary>
    ///     眩晕概率（0-100）。叠加时取较大值。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("TerrorChance", 0)
    ];

    public decimal TerrorChance
    {
        get => DynamicVars["TerrorChance"].BaseValue;
        set
        {
            DynamicVars["TerrorChance"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右上角: 眩晕概率 %
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.TopRight, $"{TerrorChance}%"),
            // 左上角: 层数
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.BottomRight, Amount.ToString())
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy) return;
        if (Owner.IsDead || Owner.Monster == null || Owner.IsStunned) return;
        if (TerrorChance <= 0m)
        {
            await PowerCmd.Remove(this);
            return;
        }

        var applier = Applier;
        if (applier is not { Player: not null }) return;

        var roll = Owner.Monster.RunRng.MonsterAi.NextFloat() * 100f;
        if (roll < (float)TerrorChance)
        {
            Flash();
            await CreatureCmd.Stun(Owner);
        }

        await PowerCmd.Decrement(this);
    }
}