using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Powers;

public class SevendrivePower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Sevendrive>();
}