using Fgo.Scripts.Cards.DerivativeMash;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

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
    ///     进入先古之民（Ancient）房间时，将玛修的衍生物卡按进化链转换升级一级。
    ///     宝具卡在 NobleDeck 中查找，普通玛修卡在主卡组（Deck）中查找；找不到源卡则跳过对应条目。
    ///     NobleDeck 为 RunPersistent 非战斗牌堆，其非 Deck 分支会依赖战斗状态（CardCmd.Transform 会 NRE），
    ///     因此手动"移除 + 创建 + 加入"；主卡组走 CardCmd.Transform 保留官方转换语义。
    /// </summary>
    public static async Task TryUpgradeDerivativeMash(Player player)
    {
        if (player == null) return;

        // ---- NobleDeck 宝具进化链: Camelot → LordCamelot → LordChaldeas ----
        // 每次进入只升一级：优先升级低级 Camelot，否则升级 LordCamelot（LordChaldeas 为终态）。
        var noblePile = CardPile.Get(FgoEnums.NobleDeck, player);
        if (noblePile != null)
        {
            var camelot = noblePile.Cards.FirstOrDefault(card => card is Camelot);
            if (camelot != null)
            {
                await ReplaceInNonCombatPile(player, noblePile, camelot, ModelDb.Card<LordCamelot>());
            }
            else
            {
                var lordCamelot = noblePile.Cards.FirstOrDefault(card => card is LordCamelot);
                if (lordCamelot != null)
                    await ReplaceInNonCombatPile(player, noblePile, lordCamelot, ModelDb.Card<LordChaldeas>());
            }
        }

        // ---- 主卡组雪花进化链: WallOfSnowflakes → VeneratedWallOfSnowflakes → VeneratedShieldOfSnowflakes ----
        var wall = player.Deck.Cards.FirstOrDefault(card => card is WallOfSnowflakes);
        if (wall != null)
        {
            await CardCmd.Transform(wall,
                player.RunState.CreateCard(ModelDb.Card<VeneratedWallOfSnowflakes>(), player),
                CardPreviewStyle.None);
        }
        else
        {
            var veneratedWall = player.Deck.Cards.FirstOrDefault(card => card is VeneratedWallOfSnowflakes);
            if (veneratedWall != null)
                await CardCmd.Transform(veneratedWall,
                    player.RunState.CreateCard(ModelDb.Card<VeneratedShieldOfSnowflakes>(), player),
                    CardPreviewStyle.None);
        }
    }

    /// <summary>
    ///     在非战斗牌堆（如 NobleDeck）中把 source 替换为 targetCanonical 的可变实例。
    ///     手动移除源卡 + 创建目标卡 + 加入牌堆，绕开 CardCmd.Transform 对战斗状态的依赖。
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

    private static Player? CurrentPlayer()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        return LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault();
    }
}