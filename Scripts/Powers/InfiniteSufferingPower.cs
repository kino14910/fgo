using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     在下个回合开始时获得暴击威力。
///     Amount 表示持续回合数（固定为1），触发后移除自身。
///     右上角标显示暴击威力百分比，右下角标显示剩余回合数 (Amount)。
/// </summary>
public class InfiniteSufferingPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    private const int CritDamagePercent = 30;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/CriticalDamagePower.png",
        "res://Fgo/images/powers/big/CriticalDamagePower.png"
    );

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.TopRight, CritDamagePercent.ToString()),
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.BottomRight, Amount.ToString())
        ];
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner, CritDamagePercent, Owner, null);
        await PowerCmd.Remove(this);
    }
}