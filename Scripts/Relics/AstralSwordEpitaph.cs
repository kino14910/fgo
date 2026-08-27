using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

/// <summary>
///     星剑的墓志铭（Astral Sword Epitaph）: [II] 的默认好感从 0 变为 1
///     （即战斗开始时好感度不清空为 0，而是置为 1）。
///     实际逻辑见 <see cref="Fgo.Scripts.Singletons.FgoPlayerState.ResetAffectionForCombat" />。
/// </summary>
public class AstralSwordEpitaph : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
}