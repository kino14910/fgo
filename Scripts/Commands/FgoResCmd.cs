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
        await ModelDb.Singleton<FgoPlayerResources>().ModifyNp(amount, player);
    }

    /// <summary>
    ///     Adds NP to the card's owner using the card's NP variable.
    /// </summary>
    public static async Task ModifyNp(CardModel card)
    {
        await ModifyNp(card.DynamicVars.EvaluateValueOrDefault("Np"), card.Owner);
    }
    
    public static async Task ResetNp()
    {
        await ModelDb.Singleton<FgoPlayerResources>().Reset();
        await Task.CompletedTask;
    }

    public static async Task ModifyStars(decimal amount, Player? player)
    {
        await ModifyStars((int)amount, player);
    }

    public static async Task ModifyStars(int amount, Player? player)
    {
        await ModelDb.Singleton<FgoPlayerResources>().ModifyStars(amount, player);
    }

    /// <summary>
    ///     Adds Stars to the card's owner using the card's Star variable.
    /// </summary>
    public static async Task ModifyStars(CardModel card)
    {
        await ModifyStars(card.DynamicVars.EvaluateValueOrDefault("Stars"), card.Owner);
    }

    public static async Task ResetStars()
    {
        await ModelDb.Singleton<FgoPlayerResources>().ResetStars();
    }
}