using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class RoadlessCamelot : NobleCardModel
{
    public RoadlessCamelot() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(24, 8);
        WithVar("Curse", 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitCount(3)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 给予所有敌人3层诅呪
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<CursePower>(choiceContext, enemy, DynamicVars["Curse"].BaseValue, Owner.Creature,
                this);
    }
}