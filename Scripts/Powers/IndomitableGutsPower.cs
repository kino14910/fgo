using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

public class IndomitableGutsPower : NonStackableGutsPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Indomitable>();
}