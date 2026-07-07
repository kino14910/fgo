using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.Colorless;

public class PoisonousDagger : FgoColorlessCard
{
    public PoisonousDagger() : base(0, CardType.Attack,
        CardRarity.Token, TargetType.AnyEnemy)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithDamage(2);
        WithPower<PoisonPower>(2, 2);
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
        await PowerCmd.Apply<PoisonPower>(choiceContext, play.Target!, DynamicVars[typeof(PoisonPower).Name].BaseValue,
            Owner.Creature, this);
    }
}