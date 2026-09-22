using Fgo.Scripts.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower]
public class HeroCreationTempStrengthPowerPower : TempStrengthPower<HeroCreation>
{
    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/BusterPower.png",
        "res://Fgo/images/powers/big/BusterPower.png"
    );
}