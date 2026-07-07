using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.Colorless;

public class GrandOrder : FgoColorlessCard
{
    public GrandOrder() : base(1, CardType.Attack,
        CardRarity.Rare, TargetType.AllEnemies)
    {
        WithDamage(9999);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_heavy")
            .Execute(choiceContext);
    }
}