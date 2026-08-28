using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

/// <summary>
///     星剑的墓志铭: [II] 的默认好感由 0 变为 10（逻辑见
///     <see cref="Fgo.Scripts.Singletons.FgoPlayerState.ResetAffectionForCombat" />）。
/// </summary>
public class AstralSwordEpitaph : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
}