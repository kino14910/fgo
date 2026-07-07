using System.Collections.Concurrent;
using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Fgo.Scripts.Utils;

public static class FgoCardActions
{
    // 按 Player 存储强制 NP 卡，避免多人游戏下的竞态
    private static readonly ConcurrentDictionary<Player, CardModel> ForcedNpCards = new();

    public static CardModel CreateGeneratedCopy(CardModel card, bool free = false, bool exhaust = false)
    {
        var copy = card.CreateDupe();
        if (free) copy.SetToFreeThisCombat();
        if (exhaust) copy.AddKeyword(CardKeyword.Exhaust);
        return copy;
    }

    public static CardModel CreateCard<T>(bool upgraded = false, bool free = false, bool exhaust = false)
        where T : CardModel
    {
        var card = ModelDb.Card<T>().ToMutable();
        if (upgraded && card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        if (free) card.SetToFreeThisCombat();
        if (exhaust) card.AddKeyword(CardKeyword.Exhaust);
        return card;
    }

    public static async Task AddToPile(CardModel card, PileType pile, CardPilePosition position = CardPilePosition.Top)
    {
        await CardPileCmd.AddGeneratedCardToCombat(card, pile, card.Owner, position);
    }

    public static async Task AddCopiesToHand(IEnumerable<CardModel> cards, bool free = false, bool exhaust = false)
    {
        foreach (var card in cards)
            await AddToPile(CreateGeneratedCopy(card, free, exhaust), PileType.Hand);
    }

    public static async Task AddRandomAttacksToHand(CardModel source, int amount, bool free = true,
        bool exhaust = false)
    {
        if (amount <= 0) return;

        var owner = source.Owner;
        var pool = owner.Character.CardPool
            .GetUnlockedCards(owner.UnlockState, owner.RunState.CardMultiplayerConstraint)
            .Where(card => card is { Type: CardType.Attack, CanBeGeneratedInCombat: true });
        var generated = CardFactory.GetDistinctForCombat(owner, pool, amount, owner.RunState.Rng.CombatCardGeneration);
        var attacks = generated
            .Select(card =>
            {
                var copy = card.ToMutable();
                if (free) copy.SetToFreeThisCombat();
                if (exhaust) copy.AddKeyword(CardKeyword.Exhaust);
                return copy;
            })
            .ToList();

        if (attacks.Count > 0)
            await CardPileCmd.AddGeneratedCardsToCombat(attacks, PileType.Hand, source.Owner, CardPilePosition.Top);
    }

    public static async Task AutoPlayCard(PlayerChoiceContext choiceContext, CardModel card, Player owner)
    {
        var target = card.TargetType == TargetType.AnyEnemy
            ? card.CombatState?.HittableEnemies.FirstOrDefault()
            : null;

        await CardCmd.AutoPlay(choiceContext, card, target, AutoPlayType.Default, false, true);
    }

    public static void BoostDamage(CardModel card, decimal amount)
    {
        if (amount == 0m) return;

        foreach (var damageVar in card.DynamicVars.Values.OfType<DamageVar>())
            damageVar.BaseValue += amount;
    }

    /// <summary>
    ///     强制下一张 NP 卡为指定类型。
    /// </summary>
    public static void ForceNextNpCard<T>(Player owner) where T : NobleCardModel
    {
        ForcedNpCards[owner] = ModelDb.Card<T>();
    }

    /// <summary>
    ///     强制下一张 NP 卡为 HollowHeartAlbion（默认行为）。
    /// </summary>
    public static void ForceNextNpCard(Player owner)
    {
        ForcedNpCards[owner] = ModelDb.Card<HollowHeartAlbion>();
    }

    public static NobleCardModel? TakeForcedNpCard(Player owner)
    {
        if (!ForcedNpCards.TryGetValue(owner, out var card) || card is not NobleCardModel forcedNpCard)
            return null;
        ForcedNpCards.TryRemove(owner, out _);
        return forcedNpCard;
    }

    public static void ClearForcedNpCard(Player owner)
    {
        ForcedNpCards.TryRemove(owner, out _);
    }

    /// <summary>
    ///     清除所有玩家的强制 NP 卡（用于战斗重置）。
    /// </summary>
    public static void ClearAllForcedNpCards()
    {
        ForcedNpCards.Clear();
    }

    public static int HandSize(Player owner)
    {
        return owner.PlayerCombatState?.Hand.Cards.Count ?? 0;
    }
}