using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class TheOneWhoSawItAll : FgoCardModel
{
    public TheOneWhoSawItAll() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("DamageBoost", 3, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var attack = Owner.PlayerCombatState!.DrawPile.Cards
            .Where(card => card.Type == CardType.Attack)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();
        if (attack == null) return;

        FgoCardActions.BoostDamage(attack, DynamicVars["DamageBoost"].IntValue);
        await CardPileCmd.Add(attack, PileType.Hand, CardPilePosition.Top, this, true);
    }
}