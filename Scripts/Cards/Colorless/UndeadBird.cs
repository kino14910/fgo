using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless;

public class UndeadBird : FgoColorlessCard
{
    public UndeadBird() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithVar("Cards", 3, 2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var owner = Owner;
        var pool = owner.Character.CardPool.GetUnlockedCards(owner.UnlockState,
            owner.RunState.CardMultiplayerConstraint);
        pool = pool.Where(card => card.DynamicVars.ContainsKey("H")).ToList();
        var generated = CardFactory.GetDistinctForCombat(owner, pool, DynamicVars["Cards"].IntValue,
            owner.RunState.Rng.CombatCardGeneration);
        var cards = generated
            .Select(card => card.ToMutable())
            .ToList();
        if (cards.Count > 0)
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner, CardPilePosition.Top);
    }
}