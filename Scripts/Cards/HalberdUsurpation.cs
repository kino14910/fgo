using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HalberdUsurpation() : FgoCardModel(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("Usurpation", 15,
            (card, target) => card.DynamicVars["ExtraDamage"].BaseValue + (target?.GetPower<StrengthPower>()?.Amount ?? 0))
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Usurpation"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("Usurpation"))
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);

        var enemyStrength = play.Target!.GetPowerAmount<StrengthPower>();
        if (enemyStrength > 0)
            await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target!, -enemyStrength * 2, Owner.Creature, this);
    }
}