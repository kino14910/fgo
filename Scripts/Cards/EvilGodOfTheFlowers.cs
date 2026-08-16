using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.CardTags;

namespace Fgo.Scripts.Cards;

public class EvilGodOfTheFlowers() : FgoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var allUnlocked = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);

        var foreignerCards = allUnlocked
            .Where(c => c.HasModCardTag(FgoTags.Foreigner))
            .ToList();

        var ownType = GetType();
        foreignerCards = foreignerCards
            .Where(c => c.GetType() != ownType)
            .ToList();

        if (foreignerCards.Count == 0)
            return;

        var takeAmount = Math.Min(3, foreignerCards.Count);
        var options = CardFactory.GetDistinctForCombat(
            Owner,
            foreignerCards,
            takeAmount,
            Owner.RunState.Rng.CombatCardGeneration).ToList();

        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext,
            options,
            Owner);

        if (selected != null)
        {
            selected.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
        }
    }
}