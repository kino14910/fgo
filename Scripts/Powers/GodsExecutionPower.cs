using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Powers;

public class GodsExecutionPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<GodsExecution>();

    protected override bool IsPositive => false;
}