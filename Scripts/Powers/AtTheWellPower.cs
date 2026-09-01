using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class AtTheWellPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AfterDurationDebuffPower.png",
        "res://Fgo/images/powers/big/AfterDurationPower.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<GutsPower>(choiceContext, Owner, Amount, Owner, null);
        await FgoResCmd.ModifyNp(80, player);
        await CreatureCmd.Kill(Owner);
        await PowerCmd.Remove(this);
    }
}