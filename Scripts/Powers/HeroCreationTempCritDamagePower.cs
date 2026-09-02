using Fgo.Scripts.Cards;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower]
public class HeroCreationTempCritDamagePower : ModTemporaryAppliedPowerTemplate<HeroCreation, CriticalDamagePower>
{
    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/CriticalDamagePower.png",
        "res://Fgo/images/powers/big/CriticalDamagePower.png"
    );
}