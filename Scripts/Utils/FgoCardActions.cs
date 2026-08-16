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

    public static async Task AddCopiesToHand(IEnumerable<CardModel> cards, bool free = false, bool exhaust = false)
    {
        foreach (var card in cards)
            await AddToPile(CreateGeneratedCopy(card, card.Owner, exhaust), PileType.Hand);
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