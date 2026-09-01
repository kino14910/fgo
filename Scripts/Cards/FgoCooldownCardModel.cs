using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     【冷却】卡牌基类。
///     卡牌描述中显示 [gold]冷却[/gold]{Cooldown}，表示当前剩余冷却：
///     - 当前冷却 &gt; 0 时本卡不可打出（通过重写 <see cref="IsPlayable" /> 控制，不动态切换 Unplayable 关键词）；
///     - 玩家每打出一张牌，本卡冷却 -1（由 FgoBattleHooks.AfterCardPlayedLate 驱动）；
///     - 冷却降到 0 后才能打出；
///     - 打出后冷却重置为 <see cref="CooldownMax" />，重新进入冷却。
///     数值保存在卡牌实例 DynamicVars 中，战斗内跨回合/跨牌堆持久，战斗开始时统一重置。
/// </summary>
public abstract class FgoCooldownCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FgoCardModel(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    /// <summary>基础冷却值。打出后冷却重置回此值。</summary>
    public abstract int CooldownMax { get; }

    /// <summary>
    ///     请在 <see cref="AdditionalCanonicalVars" /> 中追加各自的额外变量。
    /// </summary>
    protected sealed override IEnumerable<DynamicVar> CanonicalVars =>
        AdditionalCanonicalVars
            .Prepend(ModCardVars.Int("CooldownMax", CooldownMax))
            .Prepend(ModCardVars.Int("Cooldown", CooldownMax)
                .WithSharedTooltip("FGO_STATIC_HOVER_TIPS_COOLDOWN"));

    /// <summary>
    ///     获取此冷却卡牌的额外变量。
    /// </summary>
    protected virtual IEnumerable<DynamicVar> AdditionalCanonicalVars => [];

    /// <summary>当前剩余冷却。</summary>
    public int CurrentCooldown => DynamicVars["Cooldown"].IntValue;

    /// <summary>当前冷却是否为 0（即可打出）。</summary>
    public bool IsReady => CurrentCooldown <= 0;

    protected override bool ShouldGlowGoldInternal => IsReady;

    protected override bool IsPlayable => base.IsPlayable && IsReady;

    /// <summary>将当前冷却重置为 <see cref="CooldownMax" />（打出后重新进入冷却）。</summary>
    public void ResetCooldown()
    {
        SetCooldown(DynamicVars["CooldownMax"].IntValue);
    }

    /// <summary>将当前冷却清零（战斗开始时可直接打出）。</summary>
    public void ReadyCooldown()
    {
        SetCooldown(0);
    }

    public void DecrementCooldown()
    {
        if (CurrentCooldown > 0)
            SetCooldown(CurrentCooldown - 1);
    }

    private void SetCooldown(int value)
    {
        DynamicVars["Cooldown"].BaseValue = Math.Max(0, value);
    }
}