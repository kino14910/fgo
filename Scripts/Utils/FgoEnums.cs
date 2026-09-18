using Fgo.Scripts.Character;
using Fgo.Scripts.Fields;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib;
using STS2RitsuLib.CardPiles;

namespace Fgo.Scripts.Utils;

public static class FgoEnums
{
    public static PileType NobleDeck { get; private set; }

    public static CardRarity NoblePhantasm { get; private set; }

    /// <summary>
    ///     〔虚数空间〕的「额外手牌」牌堆: 替代手牌显示的那一组牌。
    ///     布局用 <see cref="ModExtraHandLayoutDirection.VanillaHand" />——直接复用玩家手牌的动态扇形，
    ///     视觉上就长在手牌位置；<see cref="ModCardPileExtraHandSpec.AllowCardPlay" /> 让它走原版出牌管线。
    /// </summary>
    public static PileType VoidHand { get; private set; }

    /// <summary>
    ///     〔虚数空间〕的暂存牌堆（无界面）: 进入虚数空间时把玩家原有手牌搬到这里。
    ///     必须搬走而不能只是隐藏——否则回合结束的 <c>CombatManager.FlushPlayerHand</c> 会把它们弃掉，
    ///     "退出后把原来的手牌拿回手中"就落空了。
    /// </summary>
    public static PileType VoidHold { get; private set; }

    public static void Initialize(string modId)
    {
        NobleDeck = ModCardPileRegistry.For(modId)
            .RegisterOwned("Noble", new ModCardPileSpec
            {
                // RunPersistent: 绑定到 Player 而非 PlayerCombatState，跨战斗保留并随存档保存。
                Scope = ModCardPileScope.RunPersistent,
                // 显示在顶栏 deck 按钮旁。
                Style = ModCardPileUiStyle.TopBarDeck,
                IconPath = $"res://{modId}/images/ui/noble_deck_button.png",
                // 复用 Deck 的牌堆界面能力：点击/右键卡牌打开原版检查界面（卡牌详情）。
                View = ModCardPileViewSpec.DeckLike,
                VisibleWhen = ctx =>
                    ctx.Player is null || ctx.Player.Character is FgoCharacter
            }).PileType;

        VoidHand = ModCardPileRegistry.For(modId)
            .RegisterOwned("VoidHand", new ModCardPileSpec
            {
                Scope = ModCardPileScope.CombatOnly,
                Style = ModCardPileUiStyle.ExtraHand,
                // AllowCardPlay 会强制启用卡牌节点表示，这里显式写出表示意图。
                CardShouldBeVisible = true,
                ExtraHand = new ModCardPileExtraHandSpec
                {
                    Direction = ModExtraHandLayoutDirection.VanillaHand,
                    AllowCardPlay = true,
                    // 不叠加原版手牌的回合结束语义: 钥匙卡要跨回合保留（只有主动打出才离开虚数空间）。
                    Behaviors = ModExtraHandBehavior.None
                },
                // 不在虚数空间里就整条隐藏，避免空容器挂在手牌位置上。
                VisibleWhen = ctx =>
                    ctx.Player is null
                    || FgoField.Has(ctx.Player.Creature.CombatState, FgoFieldId.ImaginarySpace)
            }).PileType;

        VoidHold = ModCardPileRegistry.For(modId)
            .RegisterOwned("VoidHold", new ModCardPileSpec
            {
                Scope = ModCardPileScope.CombatOnly,
                Style = ModCardPileUiStyle.Headless
            }).PileType;

        NoblePhantasm = RitsuLibFramework.RegisterDynamicEnumValue<CardRarity>(modId, "NoblePhantasm");
    }
}