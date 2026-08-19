using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Fgo.Scripts.Cards;

public class DragonCore() : FgoCardModel(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner.Creature.Player;
        if (player == null) return;
        var hands = player.PlayerCombatState!.Hand.Cards
            .Where(card => card.Type != CardType.Attack)
            .ToList();
        var exhaustCount = hands.Count;

        foreach (var card in hands)
            await CardCmd.Exhaust(choiceContext, card, true);

        var source = hands.FirstOrDefault() ?? player.Deck.Cards.FirstOrDefault();
        if (source == null) return;

        var pool = Owner.Character.CardPool
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(c => c.Type == CardType.Attack);
        var cards = CardFactory.GetForCombat(Owner, pool, exhaustCount, Owner.RunState.Rng.CombatCardGeneration)
            .ToList();
        if (IsUpgraded) CardCmd.Upgrade(cards, CardPreviewStyle.None);
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner);
    }
}