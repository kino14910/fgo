using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class HalberdUsurpation() : FgoCardModel(2, CardType.Attack,
        CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(15, ValueProp.Move)];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
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