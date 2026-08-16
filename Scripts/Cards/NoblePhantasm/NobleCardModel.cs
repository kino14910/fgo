using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
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
    : FgoBaseCardModel(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    /// <summary>
    ///     便捷构造器: 不指定稀有度（默认 Rare）和 shouldShowInCardLibrary（默认 true）。
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
        $"res://Fgo/images/cards/noble/{GetType().Name}.png",
        VisualStyle: CardVisualStyle.Ancient,
        EnergyIconPath: "res://Fgo/images/ui/energy_noble_big.png"
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material，看添加人物章节的添加卡池部分
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FgoResCmd.ModifyNp(cardPlay.Card.EnergyCost.Canonical, cardPlay.Player);

        // 打出后从本次战斗移除: 不进弃牌堆、不进消耗堆，直接消失。
        // NobleDeck 中的原卡位于 RunPersistent 牌堆（非战斗 pile），OnPlay 永远不会被调用，
        // 因此 RemoveFromCombat 只影响 NP 按钮触发时加入手牌的副本。
        // 此时卡牌位于 PileType.Play（mid-play pile），RemoveFromCombat 会把它从 Play pile 移除，
        // 后续"移到 result pile"步骤因卡牌已不在任何战斗 pile 中而被跳过。
        await CardPileCmd.RemoveFromCombat(cardPlay.Card);
    }
}