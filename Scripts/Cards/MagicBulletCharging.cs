using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class MagicBulletCharging : FgoCardModel
{
    public MagicBulletCharging() : base(-1, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithVar("ExcessBonus", 5, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var energy = play.Resources.EnergySpent;
        if (energy <= 0) return;

        var attacks = Owner.PlayerCombatState!.Hand.Cards
            .Where(card => card.Type == CardType.Attack)
            .Take(energy)
            .ToList();
        if (attacks.Count == 0) return;

        var excess = energy - attacks.Count;
        if (excess > 0) FgoCardActions.BoostDamage(attacks[^1], excess * DynamicVars["ExcessBonus"].IntValue);

        foreach (var attack in attacks)
            await FgoCardActions.AutoPlayCard(choiceContext, attack, Owner);
    }
}