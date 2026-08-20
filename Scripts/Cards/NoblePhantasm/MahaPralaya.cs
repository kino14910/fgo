using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class MahaPralaya() : NobleCardModel(1, CardType.Attack, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DamagePerStatus", 8),
        ModCardVars.ComputedDamage("MahaPralayaDamage",
            static ctx => ctx.GetCardBaseValueOrDefault("DamagePerStatus") * (ctx.CombatState?.HittableEnemies
            .SelectMany(enemy => enemy.Powers)
            .Where(power => power.Type == PowerType.Debuff)
            .Select(power => power.Id)
            .Distinct()
            .Count() ?? 0))
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["DamagePerStatus"].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("MahaPralayaDamage"))
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}