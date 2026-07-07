using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ExcaliburGalatine : NobleCardModel
{
    public ExcaliburGalatine() : base(2, CardType.Attack, TargetType.Self)
    {
        WithDamage(24, 6);
        WithPower<VigorPower>(4);
        WithVar("SunlightTurns", 3);
        WithVar("CritDamage", 50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<SunlightPower>(choiceContext, Owner.Creature, DynamicVars["SunlightTurns"].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars[typeof(VigorPower).Name].BaseValue,
            Owner.Creature, this);
        if (Owner.Creature.HasPower<SunlightPower>())
            await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
                DynamicVars["CritDamage"].BaseValue, Owner.Creature, this);
    }
}