using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     恐怖 Power。
///     amount 为层数（每回合减 1），TerrorChance 为眩晕概率（0-100）。
///     每回合结束时按概率让怪物眩晕，然后层数 -1。
/// </summary>
public class TerrorPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/EveryTurnDebuffPower.png",
        "res://Fgo/images/powers/big/EveryTurnDebuffPower.png"
    );

    /// <summary>
    ///     眩晕概率（0-100）。叠加时取较大值。
    /// </summary>
    protected override IEnumerable<DynamicVar> CanonicalVars => [
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

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (side != CombatSide.Player) return;
        if (Amount <= 0) return;

        // 概率判定眩晕（RNG 来自施加者，敌人本身没有 Player）
        var applier = Applier;
        if (applier is { Player: not null } && TerrorChance > 0m)
        {
            var rng = applier.Player.RunState.Rng.Niche;
            var roll = rng.NextFloat() * 100f;
            if (roll < (float)TerrorChance)
            {
                Flash();
                await CreatureCmd.Stun(Owner);
            }
        }

        await PowerCmd.Decrement(this);
    }
}