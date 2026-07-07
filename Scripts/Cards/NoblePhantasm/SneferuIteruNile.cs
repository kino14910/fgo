using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class SneferuIteruNile : NobleCardModel
{
    public SneferuIteruNile() : base(2, CardType.Attack, TargetType.Self)
    {
        WithDamage(35, 10);
        WithPower<VulnerablePower>(3);
        WithVar("DeathChance", 12, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        foreach (var enemy in CombatState!.HittableEnemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 3m, Owner.Creature, this);
            await PowerCmd.Apply<DoomPower>(choiceContext, enemy, DynamicVars["DeathChance"].BaseValue, Owner.Creature,
                this);
        }

        await PowerCmd.Apply<WatersidePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}