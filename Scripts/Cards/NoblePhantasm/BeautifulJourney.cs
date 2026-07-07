using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BeautifulJourney : NobleCardModel
{
    public BeautifulJourney() : base(2, CardType.Attack, TargetType.Self)
    {
        WithDamage(24, 6);
        WithNp(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue * CombatState!.HittableEnemies.Count);
    }
}