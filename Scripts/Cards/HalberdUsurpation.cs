using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class HalberdUsurpation : FgoCardModel
{
    public HalberdUsurpation() : base(2, CardType.Attack,
        CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithDamage(15, 4);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var enemyStrength = play.Target!.GetPowerAmount<StrengthPower>();
        var totalDamage = (int)DynamicVars.Damage.BaseValue + enemyStrength;

        await DamageCmd.Attack(totalDamage)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_heavy")
            .Execute(choiceContext);

        if (enemyStrength > 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target!, -enemyStrength * 2, Owner.Creature, this);
    }
}