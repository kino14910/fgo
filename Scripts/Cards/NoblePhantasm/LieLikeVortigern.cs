using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LieLikeVortigern : NobleCardModel
{
    public LieLikeVortigern() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(25, 7);
        WithPower<IntangiblePower>(1);
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
            await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -2m, Owner.Creature, this);
            await PowerCmd.Apply<IntangiblePower>(choiceContext, enemy,
                DynamicVars[typeof(IntangiblePower).Name].BaseValue, Owner.Creature, this);
        }

        await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(IntangiblePower).Name].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(7m);
    }
}