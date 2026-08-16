using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Combat.Ui.ExtraCornerAmountLabels;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

/// <summary>
///     在接下来的数个回合开始时获得暴击星。
///     Amount 表示持续回合数，每回合固定获得 8 颗暴击星。
///     右上角标显示每回合暴击星数，右下角标显示剩余回合数 (Amount)。
/// </summary>
public class SwifterThanSoundPower : FgoPowerModel, IPowerExtraIconAmountLabelSpecsProvider
{
    private const int StarsPerTurn = 8;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/StarsPerTurnPower.png",
        "res://Fgo/images/powers/big/StarsPerTurnPower.png"
    );

    public IReadOnlyList<ExtraIconAmountLabelSpec> GetPowerExtraIconAmountLabelSpecs()
    {
        return
        [
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.TopRight, StarsPerTurn.ToString()),
            ExtraIconAmountLabelSpec.Plain(ExtraIconAmountLabelCorner.BottomRight, Amount.ToString())
        ];
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await FgoResCmd.ModifyStars(StarsPerTurn, player);
        await PowerCmd.Decrement(this);
    }
}