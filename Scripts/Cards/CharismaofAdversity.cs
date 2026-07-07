using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CharismaOfAdversity : FgoCardModel
{
    public CharismaOfAdversity() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
        WithDamage(6, 3);
        WithVar(new CalculationBaseVar(1m));
        WithVar(new CalculationExtraVar(1m));
        WithVar(new CalculatedVar("CalculatedHits").WithMultiplier((card, _) =>
            (decimal)Math.Floor(
                (card.Owner.Creature.MaxHp - card.Owner.Creature.CurrentHp) / 6.0)));
    }

    protected override bool ShouldGlowGoldInternal =>
        Owner.Creature.MaxHp - Owner.Creature.CurrentHp >= 12;

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var hits = (int)((CalculatedVar)DynamicVars["CalculatedHits"]).Calculate(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
            .Execute(choiceContext);
    }
}