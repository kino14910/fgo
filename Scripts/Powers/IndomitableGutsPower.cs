using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

/// <summary>
///     「不屈」（Indomitable）施加的不叠加毅力。
/// </summary>
public class IndomitableGutsPower : NonStackableGutsPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Indomitable>();
}