using Fgo.Scripts.Cards.DerivativeNemo;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class GreatVoidSeaBattle() : FgoCardModel(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    private const int BaseNp = 5;

    /// <summary>
    ///     见 <see cref="ConsumeExplorationPoints" />。
    /// </summary>
    private const int BirdThresholdNp = 25;

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

        await FgoField.Add(Owner.Creature.CombatState, FgoFieldId.ImaginarySpace, 3);
        await FgoField.Add(Owner.Creature.CombatState, FgoFieldId.Waterside, 3);

        // 〔虚数空间〕: 进入的这一刻就把所有玩家的手牌换成额外手牌（航线规划 / 上浮吧鹦鹉螺号 / 虚数脱出）。
        if (Owner.Creature.CombatState is { } combat)
            await FgoVoidHand.EnterAll(choiceContext, combat);
    }

    /// <summary>该玩家当前的基础宝具值（含探索点数换来的永久加成）。</summary>
    private static int BaseNpOf(Player? player) => BaseNp + BonusNpOf(player);

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
    ///     <para />
    ///     加成本身存放在按玩家的局内存档（<see cref="FgoRunState" />）而不是某张卡实例上，
    ///     因为给出加成的是"另一张卡"（虚数脱出），且一位玩家只会有一份。
    ///     <para />
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
