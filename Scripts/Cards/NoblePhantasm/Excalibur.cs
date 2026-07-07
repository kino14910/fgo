using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Excalibur : NobleCardModel
{
    public Excalibur() : base(2, CardType.Attack, TargetType.Self)
    {
        WithDamage(25, 7);
        WithNp(30);
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
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
    }
}