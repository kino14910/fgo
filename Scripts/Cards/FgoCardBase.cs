using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards;

/// <summary>
///     FGO 卡牌共享基类：提供 BaseLib 风格的 With* 链式构造方法、升级数值追踪与默认 AssetProfile。
///     不携带任何 RegisterCard 特性，由派生类（FgoCardModel、NobleCardModel 等）自行注册卡池。
/// </summary>
public abstract class FgoCardBase(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    private readonly List<CardKeyword> _cardKeywords = [];

    private readonly List<DynamicVar> _constructedDynamicVars = [];
    private readonly Dictionary<DynamicVar, decimal> _varUpgrades = [];

    protected sealed override IEnumerable<DynamicVar> CanonicalVars => _constructedDynamicVars;
    public sealed override IEnumerable<CardKeyword> CanonicalKeywords => _cardKeywords;
    protected sealed override HashSet<CardTag> CanonicalTags { get; } = [];

    /// <summary>
    ///     添加原始 DynamicVar，不附带升级值。
    /// </summary>
    protected FgoCardBase WithVar(DynamicVar var)
    {
        _constructedDynamicVars.Add(var);
        return this;
    }

    /// <summary>
    ///     添加名为 name 的自定义变量，可指定升级增量。
    /// </summary>
    protected FgoCardBase WithVar(string name, int baseVal, int upgrade = 0)
    {
        var v = new DynamicVar(name, baseVal);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加伤害变量（DamageVar），ValueProp.Move。
    /// </summary>
    protected FgoCardBase WithDamage(int baseVal, int upgrade = 0)
    {
        var v = new DamageVar(baseVal, ValueProp.Move);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加格挡变量（BlockVar），ValueProp.Move。
    /// </summary>
    protected FgoCardBase WithBlock(int baseVal, int upgrade = 0)
    {
        var v = new BlockVar(baseVal, ValueProp.Move);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加治疗变量（HealVar）。
    /// </summary>
    protected FgoCardBase WithHeal(int baseVal, int upgrade = 0)
    {
        var v = new HealVar(baseVal);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加抽牌变量（CardsVar）。
    /// </summary>
    protected FgoCardBase WithCards(int baseVal, int upgrade = 0)
    {
        var v = new CardsVar(baseVal);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加 PowerVar{T} 变量，变量名为 T 的类名。
    /// </summary>
    protected FgoCardBase WithPower<T>(int baseVal, int upgrade = 0) where T : PowerModel
    {
        var v = new PowerVar<T>(baseVal);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加 NP 自定义变量（用于 FGO 宝具卡）。
    /// </summary>
    protected FgoCardBase WithNp(int baseVal, int upgrade = 0)
    {
        return WithVar("NP", baseVal, upgrade);
    }

    /// <summary>
    ///     添加 Star 变量（对应 DynamicVars.Star）。
    /// </summary>
    protected FgoCardBase WithStar(int baseVal, int upgrade = 0)
    {
        return WithVar("Star", baseVal, upgrade);
    }

    /// <summary>
    ///     添加 Energy 变量（对应 DynamicVars.Energy）。
    /// </summary>
    protected FgoCardBase WithEnergy(int baseVal, int upgrade = 0)
    {
        var v = new EnergyVar(baseVal);
        _constructedDynamicVars.Add(v);
        if (upgrade != 0) _varUpgrades[v] = upgrade;
        return this;
    }

    /// <summary>
    ///     添加单个关键词。
    /// </summary>
    protected FgoCardBase WithKeyword(CardKeyword keyword)
    {
        _cardKeywords.Add(keyword);
        return this;
    }

    /// <summary>
    ///     添加计算伤害变量（对应 DynamicVars.CalculatedDamage）。
    ///     计算公式：baseVal + 1 * calculator(card, owner)。
    /// </summary>
    protected FgoCardBase WithCalculatedDamage(int baseVal, Func<CardModel, Player, int> calculator)
    {
        _constructedDynamicVars.Add(new CalculationBaseVar(baseVal));
        _constructedDynamicVars.Add(new ExtraDamageVar(1m));
        var calcVar = new CalculatedDamageVar(ValueProp.Move);
        calcVar.WithMultiplier((card, _) => calculator(card, card.Owner));
        _constructedDynamicVars.Add(calcVar);
        return this;
    }

    /// <summary>
    ///     升级时修改费用（通常为负值以降低费用）。
    /// </summary>
    protected void WithCostUpgradeBy(int delta)
    {
        EnergyCost.UpgradeBy(delta);
    }

    /// <summary>
    ///     添加多个关键词。
    /// </summary>
    protected FgoCardBase WithKeywords(params CardKeyword[] keywords)
    {
        foreach (var kw in keywords) _cardKeywords.Add(kw);
        return this;
    }

    /// <summary>
    ///     添加多个 CardTag。
    /// </summary>
    protected FgoCardBase WithTags(params CardTag[] tags)
    {
        foreach (var tag in tags) CanonicalTags.Add(tag);
        return this;
    }

    /// <inheritdoc />
    protected override void OnUpgrade()
    {
        foreach (var (var, upgrade) in _varUpgrades)
            var.UpgradeValueBy(upgrade);
    }
}