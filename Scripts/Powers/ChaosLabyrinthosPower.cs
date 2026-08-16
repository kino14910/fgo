using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Powers;

public class ChaosLabyrinthosPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<ChaosLabyrinthos>();

    protected override bool IsPositive => false;
}