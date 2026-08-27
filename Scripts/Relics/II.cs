using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

/// <summary>
///     [II]（仅作为好感度系统的载体遗物）: 显示当前[gold]好感度[/gold]。
///     好感度在战斗中累积（女神的砂糖、兽冠等来源），战斗开始时清空
///     （持有星剑的墓志铭时默认变为 1，见 <see cref="FgoPlayerState.ResetAffectionForCombat" />）。
/// </summary>
public class II : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool ShowCounter => true;

    public override int DisplayAmount => Owner != null ? FgoBattleHooks.Get(Owner).Affection : 0;

    /// <summary>
    ///     好感度变化时刷新计数显示（由 FgoPlayerState 调用）。
    /// </summary>
    public void RefreshDisplay()
    {
        RefreshDisplayOnly();
    }
}