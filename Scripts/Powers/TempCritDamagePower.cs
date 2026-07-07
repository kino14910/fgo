using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower]
public class TempCritDamagePower : ModTemporaryAppliedPowerTemplate<HeroCreation, CriticalDamagePower>
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/CriticalDamagePower.png",
        "res://Fgo/images/powers/big/CriticalDamagePower.png"
    );

    public int CritDamageAmount { get; set; }
}