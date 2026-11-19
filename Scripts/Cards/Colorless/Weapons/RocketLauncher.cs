using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

[RegisterCard(typeof(TokenCardPool))]
public class RocketLauncher() : FgoBaseCardModel(0, CardType.Attack,
    CardRarity.Token, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(8)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var damage = DynamicVars.Damage.BaseValue;

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_fire_burst")
            .Execute(choiceContext);

        var others = combat.HittableEnemies.Where(enemy => enemy != target && enemy.IsAlive).ToList();
        foreach (var enemy in others)
            await DamageCmd.Attack(damage / 2m)
                .FromCard(this, cardPlay)
                .Targeting(enemy)
                .WithNoAttackerAnim()
                .WithHitFx("vfx/vfx_fire_burst")
                .Execute(choiceContext);
    }
}