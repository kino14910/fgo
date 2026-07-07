using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.DerivativeMash;

public class RayProofKyrielight : FgoColorlessCard
{
    public RayProofKyrielight() : base(1, CardType.Attack,
        CardRarity.Status, TargetType.Self)
    {
        WithDamage(30, 10);
        WithPower<VulnerablePower>(3, 1);
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
                DynamicVars[typeof(VulnerablePower).Name].BaseValue, Owner.Creature, this);
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