using System.ComponentModel;
using Fgo.Scripts.Character;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
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
    /// <summary>
    ///     便捷构造器: 不指定稀有度（默认 Rare）和 shouldShowInCardLibrary（默认 true）。
    /// </summary>
    public NobleCardModel(int energyCost, CardType type, TargetType targetType)
        : this(FgoReflectedSettings.EnableNoCostNoblePhantasm ? 0 : energyCost, type, FgoEnums.NoblePhantasm,
            targetType)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    public override CardPoolModel Pool => ModelDb.CardPool<NobleCardPool>();
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
    // protected override CardLocation GetResultLocationForCardPlay() =>
    //     new(Owner, PileType.None, CardPilePosition.Bottom);
}