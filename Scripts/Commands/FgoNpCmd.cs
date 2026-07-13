using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Commands;

public static class FgoNpCmd
{
    public static Task AddNp(int amount)
    {
        return ModelDb.Singleton<FgoPlayerResources>().AddNp(amount, player: CurrentPlayer());
    }

    public static Task AddNp(int amount, Player player)
    {
        return ModelDb.Singleton<FgoPlayerResources>().AddNp(amount, player: player);
    }

    public static Task AddNp(int amount, PlayerChoiceContext choiceContext, Player player)
    {
        return ModelDb.Singleton<FgoPlayerResources>().AddNp(amount, choiceContext, player);
    }

    /// <summary>
    ///     Adds NP to the card's owner using the card's NP variable.
    /// </summary>
    public static Task AddNp(CardModel card)
    {
        return AddNp(card.DynamicVars["Np"].IntValue, card.Owner);
    }

    /// <summary>
    ///     Adds NP to the card's owner using the card's NP variable.
    /// </summary>
    public static Task AddNp(CardModel card, PlayerChoiceContext choiceContext)
    {
        return AddNp(card.DynamicVars["Np"].IntValue, choiceContext, card.Owner);
    }

    public static Task ResetNp()
    {
        ModelDb.Singleton<FgoPlayerResources>().Reset();
        return Task.CompletedTask;
    }

    private static Player? CurrentPlayer()
    {
        var state = CombatManager.Instance.DebugOnlyGetState();
        return LocalContext.GetMe(state) ?? state?.Players.FirstOrDefault();
    }
}