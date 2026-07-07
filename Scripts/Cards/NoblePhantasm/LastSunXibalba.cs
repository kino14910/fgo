using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class LastSunXibalba : NobleCardModel
{
    public LastSunXibalba() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(6, 2);
        WithStar(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitCount(5)
            .WithHitFx("vfx/vfx_attack_fire")
            .Execute(choiceContext);
        await FgoStarCmd.AddStars(DynamicVars.Stars.IntValue);
    }
}