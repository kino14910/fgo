using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Fgo.Scripts.Cards.Colorless;

public class Shvibzik : FgoColorlessCard
{
    public Shvibzik() : base(1, CardType.Attack,
        CardRarity.Token, TargetType.AnyEnemy)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithDamage(8, 3);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await RelicCmd.Obtain(ModelDb.Relic<GlassEye>().ToMutable(), Owner, Owner.Relics.Count);
    }
}