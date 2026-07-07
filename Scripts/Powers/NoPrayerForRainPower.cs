using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

public class NoPrayerForRainPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
}