using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     特诺奇蒂特兰的奇迹: 接下来 3 回合，每回合开始时获得 {Amount}% 宝具值。
///     仿照 NonStackableGutsPower，使用非堆叠实例 + turns 计数器控制持续时间，
///     回合结束后自动递减，归零后移除自身。
/// </summary>
public class LakeTexcocoPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("turns", 3)
    ];

    /// <summary>剩余生效回合数。</summary>
    public decimal Turns
    {
        get => DynamicVars["turns"].BaseValue;
        set
        {
            DynamicVars["turns"].BaseValue = value;
            InvokeDisplayAmountChanged();
        }
    }

    public override LocString Description =>
        new("powers", "FGO_POWER_LAKE_TEXCOCO_POWER.description");

    protected override string SmartDescriptionLocKey =>
        "FGO_POWER_LAKE_TEXCOCO_POWER.smartDescription";
    
    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/NpPerTurnPower.png",
        "res://Fgo/images/powers/big/NpPerTurnPower.png"
    );

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            // 右下角: 每回合获得的宝具值
            ExtraIconAmountLabelSpec.RichText(
                ExtraIconAmountLabelCorner.BottomRight,
                $"[green]{Amount}%[/green]"),
            // 右上角: 剩余生效回合
            ExtraIconAmountLabelSpec.Plain(
                ExtraIconAmountLabelCorner.TopRight,
                $"×{Turns}")
        ];
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await FgoResCmd.ModifyNp(Amount, player);

        Turns--;
        if (Turns <= 0)
            await PowerCmd.Remove(this);
    }
}