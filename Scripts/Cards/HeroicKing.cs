using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class HeroicKing : FgoCardModel
{
    public HeroicKing() : base(1, CardType.Attack,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithDamage(5);
        WithVar("Hits", 4, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars["Hits"].IntValue)
            .FromCard(this, play)
            .TargetingRandomOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<HeroicKingPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}