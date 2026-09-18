using Fgo.Scripts.Cards.DerivativeNemo;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class GreatVoidSeaBattle() : FgoCardModel(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    /// <summary>卡面写死的基础宝具值（百分点）。</summary>
    public const int BaseNp = 5;

    /// <summary>
    ///     基础宝具值达到此值时**首次**解锁〔好似飞鸟〕，并把加成清回 <see cref="BaseNp" /> 对应的 0。
    ///     已解锁后不再判定（也不会再清加成），见 <see cref="ConsumeExplorationPoints" />。
    /// </summary>
    public const int BirdThresholdNp = 25;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.ImaginarySpace.ToHoverTip(),
        FgoFieldId.Waterside.ToHoverTip(),
        HoverTipFactory.FromCard<RoutePlanning>(),
        HoverTipFactory.FromCard<ToTheSurfaceNautilus>(),
        HoverTipFactory.FromCard<VoidEscape>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        // 基础宝具值 = 5 +〔虚数脱出〕换来的永久加成。
        // 取 ctx.Player 而不是 Card.Owner: canonical（卡牌图鉴/收藏预览）没有 owner，也就没有加成，
        // 且 Card.Owner 的 getter 带 AssertMutable，对 canonical 取值不安全。
        ModCardVars.Computed("Np", static ctx =>
                BaseNpOf(ctx.Player),
            BaseNp)
    ];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FgoResCmd.ModifyNp(DynamicVars.EvaluateValueOrDefault("Np"), Owner);

        FgoField.Add(Owner.Creature.CombatState, FgoFieldId.ImaginarySpace, 3);
        FgoField.Add(Owner.Creature.CombatState, FgoFieldId.Waterside, 3);

        // 〔虚数空间〕: 进入的这一刻就把所有玩家的手牌换成额外手牌（航线规划 / 上浮吧鹦鹉螺号 / 虚数脱出）。
        if (Owner.Creature.CombatState is { } combat)
            await FgoVoidHand.EnterAll(choiceContext, combat);
    }

    /// <summary>该玩家当前的基础宝具值（含探索点数换来的永久加成）。</summary>
    public static int BaseNpOf(Player? player) => BaseNp + BonusNpOf(player);

    /// <summary>
    ///     该玩家是否已解锁〔好似飞鸟〕。解锁之后，每次进入〔虚数空间〕时额外手牌里都会多一张
    ///     <c>LikeABird</c>（好似飞鸟）——它是解锁的唯一用途。
    /// </summary>
    public static bool IsLikeABirdUnlocked(Player? player) =>
        player != null && Entry.RunState.Get(player).LikeABirdUnlocked;

    /// <summary>
    ///     消耗该玩家全部探索点数: 每 1 点永久 +1 基础宝具值；
    ///     首次使基础宝具值达到 <see cref="BirdThresholdNp" /> 时解锁〔好似飞鸟〕，并把加成清空
    ///     （= 基础宝具值回到 <see cref="BaseNp" />）。
    /// </summary>
    /// <remarks>
    ///     顺序刻意是「先加点、再判阈值」——按卡面「基础宝具值超过 25 时…重置为 5」，
    ///     本次加进去的点数也要算在内。
    ///     <para />
    ///     加成本身存放在按玩家的局内存档（<see cref="FgoRunState" />）而不是某张卡实例上，
    ///     因为给出加成的是"另一张卡"（虚数脱出），且一位玩家只会有一份。
    ///     <para />
    ///     〔好似飞鸟〕是**一次性奖励**：解锁之后「给鸟 + 重置」这套判定整体关闭，
    ///     再加多少点数都不会清零加成（加成会一路累积下去），
    ///     但探索点数仍然照常被消耗、宝具值仍然照常成长。
    /// </remarks>
    public static async Task ConsumeExplorationPoints(PlayerChoiceContext choiceContext, Player player)
    {
        var points = await ExplorationPointsPower.TakeAll(player.Creature);
        if (points > 0) AddBonusNp(player, points);

        if (IsLikeABirdUnlocked(player)) return;
        if (BaseNpOf(player) < BirdThresholdNp) return;

        Entry.RunState.Modify(player, data => data.LikeABirdUnlocked = true);
        AddBonusNp(player, -BonusNpOf(player));
    }

    private static int BonusNpOf(Player? player) =>
        player == null ? 0 : Entry.RunState.Get(player).GreatVoidSeaBattleBonusNp;

    private static void AddBonusNp(Player player, int delta) =>
        Entry.RunState.Modify(player, data => data.GreatVoidSeaBattleBonusNp += delta);
}
