using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Commands;

public static class FgoStarCmd
{
    public static Task AddStars(int amount)
    {
        ModelDb.Singleton<FgoPlayerResources>().AddStars(amount);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Adds Stars to the card's owner using the card's Star variable.
    /// </summary>
    public static Task AddStars(CardModel card)
    {
        return AddStars(card.DynamicVars["Star"].IntValue);
    }

    public static Task<bool> ConsumeStars(int amount)
    {
        return Task.FromResult(ModelDb.Singleton<FgoPlayerResources>().ConsumeStars(amount));
    }

    public static Task ResetStars()
    {
        var resources = ModelDb.Singleton<FgoPlayerResources>();
        resources.ConsumeStars(resources.Stars);
        return Task.CompletedTask;
    }
}