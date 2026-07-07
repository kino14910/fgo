using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class ItsInevitable : FgoCardModel
{
    public ItsInevitable() : base(1, CardType.Attack,
        CardRarity.Common, TargetType.AllEnemies)
    {
        WithDamage(4, 1);
        WithVar("Boost", 4, 1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_fire")
            .Execute(choiceContext);
        await PowerCmd.Apply<ItsInevitablePower>(choiceContext, Owner.Creature, DynamicVars.Damage.IntValue,
            Owner.Creature, this);
        var power = Owner.Creature.GetPower<ItsInevitablePower>();
        if (power != null)
            power.Boost = DynamicVars["Boost"].IntValue;
    }
}