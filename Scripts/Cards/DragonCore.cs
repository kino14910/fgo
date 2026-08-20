using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Fgo.Scripts.Cards;

public class DragonCore() : FgoCardModel(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);

        var nonAttackCards = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card.Type != CardType.Attack)
            .ToList();
        var exhaustCount = nonAttackCards.Count;
        if (exhaustCount <= 0) return;

        foreach (var card in nonAttackCards)
            await CardCmd.Exhaust(choiceContext, card);

        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack);
        var cards = CardFactory.GetForCombat(Owner, pool, exhaustCount, Owner.RunState.Rng.CombatCardGeneration)
            .ToList();
        if (IsUpgraded) CardCmd.Upgrade(cards, CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner);
    }
}