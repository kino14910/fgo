using Fgo.Scripts.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Powers;

/// <summary>
///     「纵使三度迎来落日」（SpringOfFire）施加的不叠加毅力。
/// </summary>
public class SpringOfFireGutsPower : NonStackableGutsPower
{
    public override AbstractModel OriginModel => ModelDb.Card<SpringOfFire>();
}