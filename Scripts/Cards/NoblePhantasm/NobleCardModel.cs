using Fgo.Scripts.Character;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.NoblePhantasm;

[RegisterCard(typeof(NobleCardPool), Inherit = true)]
public abstract class NobleCardModel(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true)
    : FgoBaseCardModel(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    private readonly int _baseEnergyCost = energyCost;

    /// <summary>
    ///     便捷构造器: 不指定稀有度（默认 Rare）和 shouldShowInCardLibrary（默认 true）。
    /// </summary>
    public NobleCardModel(int energyCost, CardType type, TargetType targetType)
        : this(energyCost, type, FgoEnums.NoblePhantasm,
            targetType)
    {
    }

    /// <summary>
    ///     联网对局中读取主机同步的运行时值 NetworkNoCostNoblePhantasm（开局前由主机广播，
    ///     各端一致）；单机回退到本机开关。免费时 canonical 费用为 -1，STS2 以负费用表示「无能量费用」：
    ///     NCard.UpdateEnergyCostVisuals 里 _energyIcon.Visible = cost &gt;= 0，负值即隐藏能量图标，
    ///     实际支付为 0（GetAmountToSpend 内 Math.Max(0, cost)）。注意 -1 只能走 canonical
    ///     的 CardEnergyCost「_base &lt; 0 提前返回」路径产出；TryModifyEnergyCostInCombat 钩子
    ///     返回负值会被 GetWithModifiers 末尾 Math.Max(0,…) 钳回 0，故不要从钩子下负费。
    /// </summary>
    protected override int CanonicalEnergyCost =>
        FgoConfigSync.NetworkNoCostNoblePhantasm ? -1 : _baseEnergyCost;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    public override CardPoolModel Pool => ModelDb.CardPool<NobleCardPool>();

    public override int MaxUpgradeLevel => OverchargePower.MaxOvercharge;

    public override CardAssetProfile AssetProfile => new(
        $"res://Fgo/images/cards/noble/{GetType().Name}.png",
        VisualStyle: CardVisualStyle.Ancient,
        EnergyIconPath: "res://Fgo/images/ui/energy_noble_big.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
        
    );

    // public PileType? GetResultPileTypeForCardPlay(CardModel card) => PileType.None;
    //
    protected override CardLocation GetResultLocationForCardPlay()
    {
        return new CardLocation(Owner, PileType.None, CardPilePosition.Bottom);
    }
}