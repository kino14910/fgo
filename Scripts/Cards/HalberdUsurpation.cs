using Fgo.Scripts.Powers;
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
            (card, target) =>
                card.DynamicVars["Usurpation"].BaseValue + (target?.GetPower<StrengthPower>()?.Amount ?? 0))
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Usurpation"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("Usurpation"))
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);

        var enemyStrength = cardPlay.Target.GetPowerAmount<StrengthPower>();
        if (enemyStrength > 0)
            await PowerCmd.Apply<HalberdUsurpationPower>(choiceContext, cardPlay.Target, -enemyStrength * 2,
                Owner.Creature, this);
    }
}