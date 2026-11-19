using Fgo.Scripts.Cards;
using Fgo.Scripts.Cards.Colorless.DerivativeNemo;
using Fgo.Scripts.Powers;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.CardPiles.Nodes;

namespace Fgo.Scripts.Utils;

/// <summary>
///     〔虚数空间〕的「替换手牌」机制：暂存原手牌，改用固定额外手牌。按玩家各自结算，互不影响。
/// </summary>
/// <remarks>
///     RitsuLib 的 ExtraHand 是<b>叠加</b>语义、无「接管原版手牌」开关，故需补两件事：
///     ① 原手牌搬进无界面的 <see cref="FgoEnums.VoidHold" />（否则回合结束 <c>CombatManager.FlushPlayerHand</c> 会弃掉）；
///     ② <c>FgoGlobalHud</c> 隐藏原版手牌节点，否则额外手牌与原手牌叠在一起。
///     <para>
///         <b>状态载体</b>：是否在虚数空间内 = 该玩家 <c>Creature</c> 上有没有 <see cref="ImaginarySpacePower" />。
///         它不计时，只有打出〔虚数脱出〕才会解除。Power 是游戏原生状态，随复制动作与存档同步，
///         联机时每名玩家各自持有，不需要内存名单，也不需要额外的网络广播。
///     </para>
/// </remarks>
public static class FgoVoidHand
{
    /// <summary>施加 Power 用的固定层数：不计时，只保证 <c>PowerCmd.Apply</c> 的 amount 非 0（0 会跳过施加、≤0 会被自动移除）。</summary>
    private const int EntryStacks = 1;

    public static bool IsInside(Player? player)
    {
        return player?.Creature.HasPower<ImaginarySpacePower>() == true;
    }

    /// <summary>
    ///     是否真的处在 voidHand 中（能力在身且额外手牌区还没被清空）。
    ///     额外手牌被清掉就视为已退出：即使能力因异常路径残留，也不会继续拦抽牌/弃牌。
    /// </summary>
    public static bool IsVoidHandActive(Player player)
    {
        return IsInside(player) && CardPile.Get(FgoEnums.VoidHand, player) is { IsEmpty: false };
    }

    /// <summary>只让指定玩家进入（幂等）：标记状态、暂存原手牌、补齐专用牌。</summary>
    public static async Task Enter(PlayerChoiceContext choiceContext, ICombatState combat, Player player)
    {
        if (IsInside(player)) return;

        var power = await PowerCmd.Apply<ImaginarySpacePower>(choiceContext, player.Creature, EntryStacks,
            player.Creature, null);
        if (power == null) return;

        // 进场先清一次额外手牌区：上一场次残留牌位（模型已不在牌堆、UI 没人收）不清会和新补的牌一起显示。
        PurgeVoidHandViews();

        // VoidHold 为 null 说明该玩家没有牌堆（理论上不会发生），此时仍要继续补额外手牌，否则拿到空手牌区且无法自救。
        if (CardPile.Get(FgoEnums.VoidHold, player) is { } hold)
        {
            var hand = PileType.Hand.GetPile(player);
            foreach (var card in hand.Cards.ToList())
                await CardPileCmd.Add(card, hold, CardPilePosition.Bottom, null, true);
        }

        await TopUpVoidHandCards(combat, player);
    }

    /// <summary>只让指定玩家退出：解除不抽牌状态、清掉额外手牌、还回暂存手牌。</summary>
    /// <remarks>
    ///     <b>所有退出路径都必须经这里</b>（〔虚数脱出〕/ 残留自愈）；虚数空间不会自动到期。
    ///     不抽牌由 <see cref="ImaginarySpacePower.ShouldDraw" /> 实现，退出后必须立即失效：这里无条件移除能力，
    ///     <c>RestoreHand</c> 再清掉额外手牌，两层都让钩子放行。
    /// </remarks>
    public static async Task Exit(Player player)
    {
        await PowerCmd.Remove<ImaginarySpacePower>(player.Creature);
        await RestoreHand(player);
    }

    /// <summary>
    ///     玩家回合开始：仍在虚数空间则补齐被消耗/读档丢失的专用牌；不在但有残留则清理。
    /// </summary>
    /// <remarks>
    ///     虚数空间没有倒计时，这里不决定进出；补牌保证〔虚数脱出〕始终可用，不会把玩家锁在里面。
    ///     残留清理经 <see cref="Exit" />，确保不抽牌状态一并解除。
    /// </remarks>
    public static async Task OnPlayerTurnStart(Player player)
    {
        var combat = player.Creature.CombatState;
        if (combat == null) return;

        if (!IsInside(player))
        {
            if (HasResidue(player)) await Exit(player);
            return;
        }

        await TopUpVoidHandCards(combat, player);
    }

    /// <summary>缺哪张补哪张。战斗中生成的卡必须挂 <see cref="ICombatState" />，否则打出时会抛异常。</summary>
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

    private static bool HasResidue(Player player)
    {
        return CardPile.Get(FgoEnums.VoidHand, player) is { IsEmpty: false }
               || CardPile.Get(FgoEnums.VoidHold, player) is { IsEmpty: false }
               || player.Creature.HasPower<LikeABirdPower>();
    }

    private static bool Contains<T>(CardPile pile) where T : CardModel
    {
        return pile.Cards.Any(static card => card is T);
    }

    private static Task AddToVoidHand(Player player, CardModel card)
    {
        return CardPileCmd.AddGeneratedCardToCombat(card, FgoEnums.VoidHand, player);
    }

    /// <summary>
    ///     清掉额外手牌区里「模型已不在牌堆」的残留牌位（holder）。
    /// </summary>
    /// <remarks>
    ///     RitsuLib 额外手牌容器只在牌堆移动视觉流程或出牌结束时摘牌位；
    ///     <see cref="CardPileCmd.RemoveFromCombat(CardModel, bool)" /> 的 <c>skipVisuals</c> 分支直接
    ///     <c>card.RemoveFromCurrentPile()</c>，两条都不走 ⇒ 退出时模型清掉但牌位还在容器上。
    ///     判定用「模型当前牌堆≠VoidHand」而非比较实例：出牌途中模型短暂在 Play 牌堆，但 holder 已被 reparent 走、不在区内，不会误伤。
    /// </remarks>
    private static void PurgeVoidHandViews()
    {
        if (NCombatRoom.Instance?.Ui is not { } ui) return;

        foreach (var view in ui.GetChildren().OfType<NModExtraHand>())
        {
            if (!view.Definition.PileType.Equals(FgoEnums.VoidHand)) continue;

            foreach (var holder in CollectHolders(view))
            {
                if (holder.CardNode?.Model is { } model && model.Pile?.Type.Equals(FgoEnums.VoidHand) == true)
                    continue;

                // 与 RitsuLib RemoveHolderForPileMove 同款收尾：先摘焦点再拆节点。
                if (holder.HasFocus()) holder.ReleaseFocus();
                holder.GetParent()?.RemoveChildSafely(holder);
                holder.Clear();
                holder.QueueFreeSafely();
            }
        }
    }

    /// <summary>收集额外手牌区容器下所有手牌牌位（出牌途中被 reparent 走的不在容器内，不会收到）。</summary>
    private static List<NHandCardHolder> CollectHolders(NModExtraHand view)
    {
        var holders = new List<NHandCardHolder>();
        Collect(view);
        return holders;

        void Collect(Node node)
        {
            foreach (var child in node.GetChildren())
                if (child is NHandCardHolder holder)
                    holders.Add(holder);
                else
                    Collect(child);
        }
    }

    /// <summary>清掉额外手牌与挂件、还回暂存手牌；不抽牌能力的移除由 <see cref="Exit" /> 统一负责。</summary>
    private static async Task RestoreHand(Player player)
    {
        if (player.Creature.HasPower<LikeABirdPower>())
            await PowerCmd.Remove<LikeABirdPower>(player.Creature);

        if (CardPile.Get(FgoEnums.VoidHand, player) is { IsEmpty: false } voidHand)
        {
            foreach (var card in voidHand.Cards.ToList())
                await CardPileCmd.RemoveFromCombat(card, true);
            voidHand.Clear(true);

            // 模型清完还要收掉额外手牌区牌位：RemoveFromCombat 的 skipVisuals 不走视觉流程，容器不自己摘牌位。
            PurgeVoidHandViews();
        }

        if (CardPile.Get(FgoEnums.VoidHold, player) is not { IsEmpty: false } hold) return;

        var hand = PileType.Hand.GetPile(player);
        foreach (var card in hold.Cards.ToList())
            await CardPileCmd.Add(card, hand, CardPilePosition.Bottom, null, true);
    }
}