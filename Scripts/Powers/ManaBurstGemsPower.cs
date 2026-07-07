using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class ManaBurstGemsPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AfterDurationPower.png",
        "res://Fgo/images/powers/big/AfterDurationPower.png"
    );

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await PowerCmd.Apply<StrengthPower>(
            new BlockingPlayerChoiceContext(), oldOwner, Amount, oldOwner, null);
    }
}