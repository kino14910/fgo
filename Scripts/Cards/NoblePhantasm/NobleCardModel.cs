using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
    : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    /// <summary>
    ///     便捷构造器：不指定稀有度（默认 Rare）和 shouldShowInCardLibrary（默认 true）。
    /// </summary>
    public NobleCardModel(int energyCost, CardType type, TargetType targetType)
        : this(energyCost, type, CardRarity.Token, targetType)
    {
    }

    /// <summary>
    ///     直接返回 NobleCardPool，绕过基类基于 AllCardIds.Contains 的查找。
    ///     NobleCardPool 已通过 [RegisterSharedCardPool] 注册到 AllSharedCardPools，
    ///     所以基类查找理论上也能找到；这里仍然 override 是为了性能和健壮性
    ///     （避免在 AllCardIds 缓存未填充时抛 InvalidProgramException）。
    /// </summary>
    public override CardPoolModel Pool => ModelDb.CardPool<NobleCardPool>();

    /// <summary>
    ///     使用 Ancient 视觉样式（与 rarity 解耦），让 Noble 卡以 ancient 卡面布局渲染。
    ///     配合 NobleCardHideBannerPatch 在 Reload 后隐藏 %AncientBanner，
    ///     呈现"无横幅 ancient 样式"。
    /// </summary>
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Fgo/images/cards/noble/{GetType().Name}.png",
        VisualStyle: CardVisualStyle.Ancient,
        EnergyIconPath: $"res://Fgo/images/ui/energy_noble_big.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return FgoNpCmd.AddNp(cardPlay.Card.EnergyCost.Canonical);
    }
}
