using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

public class SpringOfFireGutsPower : NonStackableGutsPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SpringOfFire>();
}