using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class InnocenceAroundight : NobleCardModel
{
    public InnocenceAroundight() : base(2, CardType.Attack, TargetType.AnyEnemy)
    {
        WithDamage(32, 8);
        WithPower<NpRatePower>(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpRatePower).Name].BaseValue, Owner.Creature, this);
        await FgoCardActions.AddToPile(FgoCardActions.CreateCard<RayHorizon>(true), PileType.Draw);
    }
}