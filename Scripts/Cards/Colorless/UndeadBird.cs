using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless;

public class UndeadBird() : ModCardTemplate(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Cards", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(2);
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