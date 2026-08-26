using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Commands;

public static class FgoResCmd
{
    public static async Task ModifyNp(decimal amount, Player? player)
    {
        await ModifyNp((int)amount, player);
    }

    public static async Task ModifyNp(int amount, Player? player)
    {
        if (player == null) return;
        await FgoBattleHooks.Get(player).ModifyNp(amount, player);
    }

    public static async Task ModifyNp(CardModel card)
    {
        await ModifyNp(card.DynamicVars.EvaluateValueOrDefault("Np"), card.Owner);
    }

    public static async Task ResetNp(Player player)
    {
        await FgoBattleHooks.Get(player).Reset();
    }

    public static async Task ModifyStars(decimal amount, Player? player)
    {
        await ModifyStars((int)amount, player);
    }

    public static async Task ModifyStars(int amount, Player? player)
    {
        if (player == null) return;
        await FgoBattleHooks.Get(player).ModifyStars(amount, player);
    }

    public static async Task ModifyStars(CardModel card)
    {
        await ModifyStars(card.DynamicVars.EvaluateValueOrDefault("Stars"), card.Owner);
    }

    public static async Task ResetStars(Player player)
    {
        await FgoBattleHooks.Get(player).ResetStars();
    }
}