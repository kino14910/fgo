using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class RayProofKyrielight() : ModCardTemplate(1, CardType.Attack,
        CardRarity.Status, TargetType.Self)
    {
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30, ValueProp.Move),
        new PowerVar<VulnerablePower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
        DynamicVars[nameof(VulnerablePower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_heavy")
            .Execute(choiceContext);
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy,
                DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);
            if (!enemy.IsPrimaryEnemy)
            {
                var buffs = enemy.Powers.Where(power => power.Type == PowerType.Buff).ToList();
                foreach (var buff in buffs)
                    await PowerCmd.Remove(buff);
            }

            await PowerCmd.Apply<IgnoresInvincibilityPower>(choiceContext, enemy, 1m, Owner.Creature, this);
        }
    }
}