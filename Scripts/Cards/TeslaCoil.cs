using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class TeslaCoil : FgoCardModel
{
    public TeslaCoil() : base(1, CardType.Attack,
        CardRarity.Common, TargetType.AnyEnemy)
    {
        WithDamage(8, 3);
        WithPower<RegenPower>(2);
        WithPower<NpRatePower>(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, DynamicVars[typeof(RegenPower).Name].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpRatePower).Name].BaseValue, Owner.Creature, this);
    }
}