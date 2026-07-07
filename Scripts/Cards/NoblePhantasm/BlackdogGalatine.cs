using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BlackdogGalatine : NobleCardModel
{
    public BlackdogGalatine() : base(1, CardType.Attack, TargetType.Self)
    {
        WithDamage(16, 4);
        WithPower<RegenPower>(6, 3);
        WithVar("Energy", 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_fire")
            .Execute(choiceContext);
        // 获得再生
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[typeof(RegenPower).Name].BaseValue,
            Owner.Creature, this);
        // 获得 [E][E]
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }
}