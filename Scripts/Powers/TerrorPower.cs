using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 玩家回合开始时判定，命中敌人紧接的下一意图，避免命中已执行意图而无事发生。
        if (Owner is not { IsDead: false } || Owner.Monster == null || Owner.IsStunned) return;
        if (TerrorChance <= 0m) return;

        var applier = Applier;
        if (applier is not { Player: not null }) return;

        // 以 TerrorChance 为概率单次掷骰；使用施法玩家的 RNG（怪物 Owner.Player 为 null）。
        var roll = applier.Player.RunState.Rng.Niche.NextFloat() * 100f;
        if (roll < (float)TerrorChance)
        {
            Flash();
            await CreatureCmd.Stun(Owner);
        }

        await PowerCmd.Decrement(this);
    }
}