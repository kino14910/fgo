using Fgo.Scripts.Cards.DerivativeMash;
using Fgo.Scripts.Cards.NoblePhantasm;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Fgo.Scripts.Utils;

public static class FgoCardActions
{
    public static CardModel CreateGeneratedCopy(CardModel card, Player owner, bool free = false, bool exhaust = false)
    {
        var copy = card.CreateDupe(owner);
        if (free) copy.SetToFreeThisCombat();
        if (exhaust) copy.AddKeyword(CardKeyword.Exhaust);
        return copy;
    }

    public static CardModel CreateCard<T>(Player owner, bool upgraded = false, bool free = false, bool exhaust = false)
        where T : CardModel
    {
        // 战斗中加卡必须用 CombatState.CreateCard<T>: 它完成 ToMutable + 设置 Owner + 注册到 CombatState + AfterCreated。
        // 直接用 ModelDb.Card<T>().ToMutable() 会得到无 Owner、未注册到 CombatState 的副本，
        // 传入 CardPileCmd.Add 会抛 InvalidOperationException("...has no owner!")。
        var card = owner.Creature.CombatState!.CreateCard<T>(owner);
        if (upgraded && card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        if (free) card.SetToFreeThisCombat();
        if (exhaust) card.AddKeyword(CardKeyword.Exhaust);
        return card;
    }

    public static async Task AddToPile(CardModel card, PileType pile, CardPilePosition position = CardPilePosition.Top)
    {
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, pile, card.Owner, position), 2.2f);
    }

    /// <summary>
    ///     复刻 CardCmd.PreviewCardPileAdd（PreviewInternal）的完整预览流程：卡牌先在屏幕中心
    ///     缩放出现并停留 time 秒，再从屏幕中心飞入 NobleDeck 顶部栏牌组。
    ///     NobleDeck 是 TopBarDeck 型 RunPersistent 牌堆：在地图/战斗外使用遗物时，玩家 creature 不在战斗与图鉴场景，
    ///     card.Owner.Creature.GetVfxContainer() 返回 null，原生 PreviewInternal 的飞行动画分支会被跳过
    ///     （只剩卡牌预览、没有飞入牌组的动画）。这里仿照其实现，仅把飞行动画的 VFX 父节点固定为
    ///     顶部栏 TrailContainer（该容器在地图/战斗中始终存在），终点由 NCardFlyVfx 内部
    ///     pileType.GetTargetPosition 解析，RitsuLib 补丁已把 NobleDeck 路由到顶部栏牌组按钮。
    /// </summary>
    public static void PreviewNoblePileAdd(CardPileAddResult result, float time = 1.5f)
    {
        if (!result.success || result.cardAdded?.Pile == null || !LocalContext.IsMine(result.cardAdded))
            return;

        var card = result.cardAdded;
        var trailContainer = NRun.Instance?.GlobalUi?.TopBar?.TrailContainer as Control;
        var previewContainer = NRun.Instance?.GlobalUi?.CardPreviewContainer as Control;
        if (trailContainer == null || previewContainer == null)
        {
            CardCmd.PreviewCardPileAdd(result, time);
            return;
        }

        if (NCard.Create(card) is not { } node)
            return;

        // 屏幕中心预览容器，卡牌缩放出现。
        previewContainer.AddChildSafely(node);
        node.UpdateVisuals(card.Pile.Type, CardPreviewMode.Normal);

        var tween = node.CreateTween();
        tween.TweenProperty(node, "scale", Vector2.One, 0.25)
            .From(Vector2.Zero).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() =>
        {
            // 停留 time 秒后飞入顶部栏牌组，VFX 父节点固定用 TrailContainer。
            if (NCardFlyVfx.Create(node, card.Pile.Type, isAddingToPile: true, card.Owner.Character.TrailPath) is
                { } fly)
            {
                trailContainer.AddChildSafely(fly);
            }
            else
            {
                node.QueueFreeSafely();
            }
        })).SetDelay(time);
    }

    /// <summary>
    ///     确保指定玩家的 NobleDeck 牌堆已添加初始宝具卡（BeautifulJourney + Camelot 各一张）。
    ///     NobleDeck 是 RunPersistent 牌堆（见 FgoEnums.RegisterOwned），由 RitsuLib 按 Player 索引并随存档序列化；
    ///     在 run 生命周期事件中按玩家调用，
    ///     牌堆已有卡时直接返回，不重复添加（读档后不重新添加）。
    /// </summary>
    public static void EnsureNobleDeckSeeded(Player player)
    {
        if (player == null) return;

        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile == null || noblePile.Cards.Count > 0) return;

        noblePile.AddInternal(player.RunState.CreateCard(ModelDb.Card<BeautifulJourney>(), player));
        noblePile.AddInternal(player.RunState.CreateCard(ModelDb.Card<Camelot>(), player));
    }

    /// <summary>
    ///     进入先古之民（Ancient）房间时，将玛修的衍生物卡推进一个进化阶段（每次进入只推进一个阶段）。
    ///     第一次进化：Camelot → LordCamelot；WallOfSnowflakes → VeneratedWallOfSnowflakes（白垩之壁不变）。
    ///     第二次进化：LordCamelot → LordChaldeas；VeneratedWallOfSnowflakes → VeneratedShieldOfSnowflakes；
    ///     ObscurantWallOfChalk → ObscurantWallOfChalkA（白垩之壁随第二次进化同步升级）。
    ///     两条主进化链独立存在：阶段内源卡缺失的条目直接跳过，不抛异常；全部终态时不做任何事。
    /// </summary>
    public static async Task TryUpgradeDerivativeMash(Player player)
    {
        if (player == null) return;

        // 第一次进化
        if (HasUpgradeSource(player, FirstEvolution))
        {
            await ApplyUpgradeStage(player, FirstEvolution);
            return;
        }

        // 第二次进化
        if (HasUpgradeSource(player, SecondEvolution))
        {
            await ApplyUpgradeStage(player, SecondEvolution);
        }
    }

    /// <summary>
    ///     一次卡牌进化替换：在 PileSelector 指定的牌堆中查找 Source 卡，替换为 Target 工厂返回的 canonical 卡。
    /// </summary>
    private sealed record CardUpgrade(
        Func<Player, CardPile?> PileSelector,
        Type Source,
        Func<CardModel> Target);

    /// <summary>
    ///     一个进化阶段：进入一次 Ancient 房间推进的整体阶段。
    ///     阶段内所有源卡存在的条目都会执行（多条链共存时同步升级），源卡缺失的条目直接跳过。
    /// </summary>
    private sealed class CardUpgradeStage
    {
        public required CardUpgrade[] Upgrades { get; init; }
    }

    // ---- 第一次进化 ----
    // Camelot → LordCamelot
    // WallOfSnowflakes → VeneratedWallOfSnowflakes
    private static readonly CardUpgradeStage FirstEvolution = new()
    {
        Upgrades =
        [
            new CardUpgrade(NobleDeckPile, typeof(Camelot), () => ModelDb.Card<LordCamelot>()),
            new CardUpgrade(MainDeck, typeof(WallOfSnowflakes), () => ModelDb.Card<VeneratedWallOfSnowflakes>())
        ]
    };

    // ---- 第二次进化 ----
    // LordCamelot → LordChaldeas
    // VeneratedWallOfSnowflakes → VeneratedShieldOfSnowflakes
    // ObscurantWallOfChalk → ObscurantWallOfChalkA（白垩之壁属于本阶段的同步升级内容）
    private static readonly CardUpgradeStage SecondEvolution = new()
    {
        Upgrades =
        [
            new CardUpgrade(NobleDeckPile, typeof(LordCamelot), () => ModelDb.Card<LordChaldeas>()),
            new CardUpgrade(MainDeck, typeof(VeneratedWallOfSnowflakes),
                () => ModelDb.Card<VeneratedShieldOfSnowflakes>()),
            new CardUpgrade(MainDeck, typeof(ObscurantWallOfChalk), () => ModelDb.Card<ObscurantWallOfChalkA>())
        ]
    };

    private static CardPile? NobleDeckPile(Player player) => CardPile.Get(FgoEnums.NobleDeck, player);

    private static CardPile MainDeck(Player player) => player.Deck;

    /// <summary>
    ///     判断该阶段是否存在任一源卡，用于决定本次进入房间推进哪个阶段。
    /// </summary>
    private static bool HasUpgradeSource(Player player, CardUpgradeStage stage)
    {
        return stage.Upgrades.Any(upgrade =>
            upgrade.PileSelector(player)?.Cards.Any(card => upgrade.Source.IsInstanceOfType(card)) == true);
    }

    /// <summary>
    ///     执行阶段内所有源卡存在的升级条目；牌堆或源卡缺失时跳过该条目，不抛异常。
    /// </summary>
    private static async Task ApplyUpgradeStage(Player player, CardUpgradeStage stage)
    {
        foreach (var upgrade in stage.Upgrades)
        {
            var pile = upgrade.PileSelector(player);
            if (pile == null) continue;

            var source = pile.Cards.FirstOrDefault(card => upgrade.Source.IsInstanceOfType(card));
            if (source == null) continue;

            await ReplaceInNonCombatPile(player, pile, source, upgrade.Target());
        }
    }

    /// <summary>
    ///     在非战斗牌堆（如 NobleDeck、主卡组 Deck）中把 source 替换为 targetCanonical 的可变实例。
    ///     手动移除源卡 + 创建目标卡 + 加入牌堆（低层 API，不做 removability 检查）：
    ///     既绕开 CardCmd.Transform 对战斗状态的依赖，也允许替换带 Eternal 关键字的卡
    ///     （进化链起点的防移除保护不应阻止自身进化）。
    /// </summary>
    private static async Task ReplaceInNonCombatPile(Player player, CardPile pile, CardModel source,
        CardModel targetCanonical)
    {
        var replacement = player.RunState.CreateCard(targetCanonical, player);
        source.RemoveFromCurrentPile();
        pile.AddInternal(replacement);
        await Task.CompletedTask;
    }

    public static async Task AddCopiesToHand(IEnumerable<CardModel> cards, bool free = false, bool exhaust = false)
    {
        foreach (var card in cards)
            await AddToPile(CreateGeneratedCopy(card, card.Owner, free, exhaust), PileType.Hand);
    }

    public static void BoostDamage(CardModel card, decimal amount)
    {
        if (amount == 0m) return;

        foreach (var damageVar in card.DynamicVars.Values.OfType<DamageVar>())
            damageVar.BaseValue += amount;
    }
}