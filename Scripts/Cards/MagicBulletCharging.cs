using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class MagicBulletCharging() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override bool HasEnergyCostX => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("ExcessBonus", 5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["ExcessBonus"].UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var energy = ResolveEnergyXValue();
        if (energy <= 0) return;

        var attacks = Owner.PlayerCombatState!.Hand.Cards
            .Where(card => card.Type == CardType.Attack)
            .Take(energy)
            .ToList();
        if (attacks.Count == 0) return;

        var excess = energy - attacks.Count;
        if (excess > 0) FgoCardActions.BoostDamage(attacks[^1], excess * DynamicVars["ExcessBonus"].BaseValue);

        foreach (var attack in attacks)
        {
            var target = attack.TargetType == TargetType.AnyEnemy
                ? attack.CombatState?.HittableEnemies.FirstOrDefault()
                : null;
            await CardCmd.AutoPlay(choiceContext, attack, target, AutoPlayType.Default, true);
        }
    }
}