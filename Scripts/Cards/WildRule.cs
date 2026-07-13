using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class WildRule() : FgoCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move),
        new PowerVar<VulnerablePower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

    protected override bool ShouldGlowGoldInternal =>
        CombatState?.HittableEnemies.Any(e => e.GetPowerAmount<StrengthPower>() > 0) ?? false;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_bite")
            .Execute(choiceContext);

        await CreatureCmd.Heal(Owner.Creature, 3m, false);

        if (play.Target?.GetPowerAmount<StrengthPower>() > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target!, -1m, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target!,
                DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
        }
    }
}