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
using STS2RitsuLib.Utils;

namespace Fgo.Scripts.Utils;

/// <summary>
///     〔虚数空间〕的「替换手牌」机制: 暂存原手牌，改用固定额外手牌。
/// </summary>
/// <remarks>
///     <para>
///         RitsuLib 的 <see cref="STS2RitsuLib.CardPiles.ModCardPileUiStyle.ExtraHand" /> 是<b>叠加</b>语义，
///         没有「接管原版手牌」的开关，因此要补两件事: ① 把原版手牌搬进无界面的
///         <see cref="FgoEnums.VoidHold" />（否则回合结束 <c>CombatManager.FlushPlayerHand</c> 会弃掉）；
///         ② 由 <c>FgoGlobalHud</c> 隐藏原版手牌节点，否则额外手牌会与原手牌叠在一起。
///         额外手牌为固定三张〔航线规划〕〔上浮吧鹦鹉螺号〕〔虚数脱出〕＋解锁后的〔好似飞鸟〕，
///         都只在这里产出（Token 稀有度、不进图鉴），缺了会在回合开始补齐。
///     </para>
///     <para>
///         <b>多人下的同步约定</b>: 〔虚数空间〕是战场级场地，进入/退出必须<b>全体同时发生</b>。
///         因此：
///         <list type="bullet">
///             <item>进入只由 <see cref="EnterAll" /> 触发，一次给齐 <c>combat.Players</c> 的所有人；</item>
///             <item>
///                 退出只由 <see cref="ExitAll" />（主动打出〔虚数脱出〕）或
///                 <see cref="ExitIfInactive" />（场地回合数归零）触发，两者都遍历全体；
///             </item>
///             <item>
///                 每名玩家各自在 <c>AfterPlayerTurnStart</c> 跑一次 <see cref="Reconcile" />，
///                 它<b>只做幂等自愈</b>（补齐缺牌 / 归还残留暂存），不负责决定进出时机——
///                 否则「谁先开始回合谁先进去」，同一次进入会被切成不同步的碎片。
///             </item>
///         </list>
///     </para>
///     <para>
///         <b>跨端一致性</b>: 所有状态变更都发生在被复制的游戏动作内（出牌 <c>OnPlay</c>、回合钩子），
///         各端各跑一遍同一份逻辑，遍历顺序取 <c>ICombatState.Players</c> 的确定顺序，因此无需 Sidecar 广播。
///     </para>
/// </remarks>
public static class FgoVoidHand
{
    /// <summary>各战斗的〔虚数空间〕参与状态（进入过场次 + 成员名单）。</summary>
    private static readonly AttachedState<ICombatState, FgoVoidSpaceState> States = new(() => new FgoVoidSpaceState());

    /// <summary>该战斗当前是否处于〔虚数空间〕中。</summary>
    public static bool IsActive(ICombatState? combat) => Of(combat) is { Active: true };

    /// <summary>该玩家当前是否身处〔虚数空间〕（= 已完成手牌替换）。</summary>
    public static bool IsInside(ICombatState? combat, Player player) => Of(combat)?.IsMember(player) == true;

    /// <summary>
    ///     进入虚数空间（幂等）: 把该玩家当前手牌搬进暂存牌堆，并补齐三张虚数空间专用牌。
    /// </summary>
    /// <remarks>
    ///     幂等由 <see cref="FgoVoidSpaceState.IsMember" /> 把关，而不是靠「暂存牌堆空不空」：
    ///     重复进入（两张〔虚数大海战〕先后打出）若再次搬手牌，会把玩家在虚数空间里
    ///     新抽到的一手牌也卷走；<b>而那手牌应该在退出时留下</b>，不该被二次暂存。
    ///     同理〔虚数脱出〕之后场地已移除，此时补牌只会造出无人能出的孤牌。
    /// </remarks>
    public static async Task Enter(PlayerChoiceContext choiceContext, ICombatState combat, Player player)
    {
        var state = States.GetOrCreate(combat);
        if (state.IsMember(player)) return;

        state.Join(player);
        
        // VoidHold 为 null 说明该玩家没有牌堆（理论上不会发生），此时仍要继续补额外手牌，
        // 否则玩家会拿到一个空手牌区且无法自救。
        if (CardPile.Get(FgoEnums.VoidHold, player) is { } hold)
        {
            var hand = PileType.Hand.GetPile(player);
            foreach (var card in hand.Cards.ToList())
                await CardPileCmd.Add(card, hold, CardPilePosition.Bottom, null, true);
        }

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
    ///     退出虚数空间（幂等）: 清掉额外手牌、移除〔好似飞鸟〕，并把暂存的原手牌还回手中。
    /// </summary>
    public static async Task Exit(ICombatState combat, Player player)
    {
        var state = States.GetOrCreate(combat);
        if (!state.IsMember(player)) return;

        state.Leave(player);
        await RestoreHand(player);
    }

    /// <summary>
    ///     全体退出虚数空间: 任何一名玩家打出〔虚数脱出〕都会走到这里，所有人一起离开。
    /// </summary>
    public static async Task ExitAll(ICombatState combat)
    {
        foreach (var player in combat.Players)
            await Exit(combat, player);
        States.GetOrCreate(combat).Reset();
    }

    /// <summary>
    ///     场地到期后的兜底退出: 〔虚数空间〕的层数已归零（或被别的方式移除）时，让所有人离开。
    /// </summary>
    /// <remarks>
    ///     挂在 side 级钩子上（每轮只跑一次，见 <c>FgoFieldEffects.OnPlayerSideTurnStart</c>），
    ///     赶在递减之后、任何玩家抽牌之前。这样「场次结束」这件事在同一份复制动作里对所有人
    ///     同时发生；若放任它由各玩家自己的 <see cref="Reconcile" /> 兜底，先开始回合的那位会
    ///     提前离开、后开始的那位仍留在虚数空间，同一地块场地下的手牌状态就此分叉。
    /// </remarks>
    public static async Task ExitIfInactive(ICombatState? combat)
    {
        if (combat == null) return;
        if (FgoField.Has(combat, FgoFieldId.ImaginarySpace)) return;
        if (!IsActive(combat)) return;

        await ExitAll(combat);
    }

    /// <summary>
    ///     每名玩家回合开始时的自愈式对账：只修「不该有的状态」，不决定进出时机。
    /// </summary>
    /// <remarks>
    ///     两种需修复的状态:
    ///     <list type="bullet">
    ///         <item>
    ///             还在虚数空间却少牌（打出/消耗）→ <b>只补卡、不搬手牌</b>。
    ///             此刻原版手牌里是本回合新抽的牌，正常参与回合结束弃牌；
    ///             若把它们也暂存，退出时就会凭空多出一手牌。
    ///         </item>
    ///         <item>
    ///             已不在虚数空间却仍是成员（场地被别的方式清掉、或状态记账与牌堆脱节）
    ///             → 归还暂存手牌并摘掉成员身份。
    ///         </item>
    ///     </list>
    /// </remarks>
    public static async Task Reconcile(ICombatState combat, Player player)
    {
        if (!FgoField.Has(combat, FgoFieldId.ImaginarySpace))
        {
            // 场地没了但人还在名单里：按退出处理（幂等，名单外直接返回）。
            await Exit(combat, player);
            return;
        }

        // 场地仍在，但该玩家不在名单里 = 中途加入战斗的玩家（各端名单可能不同步）。
        // 把他补进名单并补齐手牌，否则他会看着空荡荡的原版手牌、拿不到〔虚数脱出〕。
        var state = States.GetOrCreate(combat);
        if (!state.IsMember(player))
        {
            await Enter(new BlockingPlayerChoiceContext(), combat, player);
            return;
        }

        await TopUpVoidHandCards(combat, player);
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

    private static FgoVoidSpaceState? Of(ICombatState? combat) =>
        combat == null ? null : States.GetValueOrDefault(combat);

    private static bool Contains<T>(CardPile pile) where T : CardModel =>
        pile.Cards.Any(static card => card is T);

    private static Task AddToVoidHand(Player player, CardModel card) =>
        CardPileCmd.AddGeneratedCardToCombat(card, FgoEnums.VoidHand, player);

    private static async Task RestoreHand(Player player)
    {
        // 以下能力都是「人在虚数空间」的挂件，离开即失效——放在这里而不是散落在各调用点，
        // 是因为三条退出路径（主动打出〔虚数脱出〕/ 场地到期 / Reconcile 自愈）都汇聚到本方法，
        // 能力清理与手牌归还在同一处收口，不会出现「牌还了但能力还在」的半退出状态。

        // 〔好似飞鸟〕: 能力在身 ⇔ 人在虚数空间。
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
