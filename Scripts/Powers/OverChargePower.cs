using MegaCrit.Sts2.Core.Entities.Powers;

namespace Fgo.Scripts.Powers;

public class OverChargePower : FgoPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}