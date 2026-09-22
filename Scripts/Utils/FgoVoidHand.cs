using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.DerivativeNemo;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Utils;

/// <summary>
///     〔虚数空间〕的「替换手牌」机制: 暂存原手牌，改用固定额外手牌。
/// </summary>
/// <remarks>
///     RitsuLib 的 <see cref="STS2RitsuLib.CardPiles.ModCardPileUiStyle.ExtraHand" /> 是**叠加**语义，
///     没有"接管原版手牌"的开关，因此要补两件事: ① 把原版手牌搬进无界面的
///     <see cref="FgoEnums.VoidHold" />（否则回合结束 <c>CombatManager.FlushPlayerHand</c> 会弃掉）；
///     ② 由 <c>FgoGlobalHud</c> 隐藏原版手牌节点，否则额外手牌会与原手牌叠在一起。
///     额外手牌为固定三张〔航线规划〕〔上浮吧鹦鹉螺号〕〔虚数脱出〕＋解锁后的〔好似飞鸟〕，
///     都只在这里产出（Token 稀有度、不进图鉴），缺了会在回合开始补齐。
///     退出时（<see cref="ExitAll" /> 或对账）还手牌并移除 <see cref="LikeABirdPower" />——
///     〔好似飞鸟〕在身 ⇔ 人在虚数空间。所有改动都在被复制的游戏动作内，无需额外跨端同步。
/// </remarks>
public static class FgoVoidHand
{
    /// <summary>
    ///     进入虚数空间: 把该玩家当前手牌搬进暂存牌堆，并把三张虚数空间专用牌放进额外手牌。
    /// </summary>
    public static async Task Enter(PlayerChoiceContext choiceContext, ICombatState combat, Player player)
    {
        await PowerCmd.Apply<NoDrawPower>(choiceContext, player.Creature, 1m, player.Creature, null);
        if (CardPile.Get(FgoEnums.VoidHold, player) is not { } hold) return;

        var hand = PileType.Hand.GetPile(player);
        foreach (var card in hand.Cards.ToList())
            await CardPileCmd.Add(card, hold, CardPilePosition.Bottom, null, true);

        await TopUpVoidHandCards(combat, player);
    }

    /// <summary>
    ///     对战场上的每名玩家执行一次进入（牌堆内容是游戏状态，必须在被复制的动作内改）。
    /// </summary>
    public static async Task EnterAll(PlayerChoiceContext choiceContext, ICombatState combat)
    {
        foreach (var player in combat.Players)
            await Enter(choiceContext, combat, player);
    }

    /// <summary>
    ///     退出虚数空间: 清掉额外手牌，并把暂存的原手牌还回手中。
    /// </summary>
    public static async Task ExitAll(ICombatState combat)
    {
        foreach (var player in combat.Players)
            await RestoreHand(player);
    }

    /// <summary>
    ///     每名玩家回合开始时的自愈式对账。
    /// </summary>
/// <remarks>
///     两种需修复的状态: ① 还在虚数空间却少牌（打出/消耗）→ 只补卡、不搬手牌（当前手上是新抽的牌，回合结束正常弃）；
///     ② 已不在虚数空间但暂存牌堆仍有牌（如场地被别的方式清掉）→ 还手牌。
/// </remarks>
    public static async Task Reconcile(ICombatState combat, Player player)
    {
        if (FgoField.Has(combat, FgoFieldId.ImaginarySpace))
        {
            await TopUpVoidHandCards(combat, player);
            return;
        }

        if (CardPile.Get(FgoEnums.VoidHold, player) is { IsEmpty: false })
            await RestoreHand(player);
    }

    /// <summary>
    ///     缺哪张补哪张。战斗中生成的卡必须挂在 <see cref="ICombatState" /> 上:
    ///     <c>CardModel.OnPlayWrapper</c> 手动出牌那条分支会调 <c>CardPileCmd.AddDuringManualCardPlay</c>，
    ///     那里用 <c>CombatState.ContainsCard</c> 校验；而 <c>RunState.CreateCard</c> 只把卡登记进运行存档的卡表，
    ///     玩家一打出就抛 "must be added to a CombatState before playing it."。
    /// </summary>
    private static async Task TopUpVoidHandCards(ICombatState combat, Player player)
    {
        if (CardPile.Get(FgoEnums.VoidHand, player) is not { } voidHand) return;

        if (!Contains<RoutePlanning>(voidHand))
            await AddToVoidHand(player, combat.CreateCard<RoutePlanning>(player));
        if (!Contains<ToTheSurfaceNautilus>(voidHand))
            await AddToVoidHand(player, combat.CreateCard<ToTheSurfaceNautilus>(player));
        if (!Contains<VoidEscape>(voidHand))
            await AddToVoidHand(player, combat.CreateCard<VoidEscape>(player));

        if (GreatVoidSeaBattle.IsLikeABirdUnlocked(player) && !Contains<LikeABird>(voidHand))
            await AddToVoidHand(player, combat.CreateCard<LikeABird>(player));
    }

    private static bool Contains<T>(CardPile pile) where T : CardModel =>
        pile.Cards.Any(static card => card is T);

    private static Task AddToVoidHand(Player player, CardModel card) =>
        CardPileCmd.AddGeneratedCardToCombat(card, FgoEnums.VoidHand, player);

    private static async Task RestoreHand(Player player)
    {
        // 离开〔虚数空间〕即失去〔好似飞鸟〕的能力（能力在身 ⇔ 人在虚数空间）。
        if (player.Creature.HasPower<LikeABirdPower>())
            await PowerCmd.Remove<LikeABirdPower>(player.Creature);

        if (CardPile.Get(FgoEnums.VoidHand, player) is { IsEmpty: false } voidHand)
        {
            foreach (var card in voidHand.Cards.ToList())
                await CardPileCmd.RemoveFromCombat(card, true);
            voidHand.Clear(true);
        }

        if (CardPile.Get(FgoEnums.VoidHold, player) is not { IsEmpty: false } hold) return;

        var hand = PileType.Hand.GetPile(player);
        foreach (var card in hold.Cards.ToList())
            await CardPileCmd.Add(card, hand, CardPilePosition.Bottom, null, true);
    }
}
