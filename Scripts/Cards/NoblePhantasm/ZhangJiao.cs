using Fgo.Scripts.Fields;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class ZhangJiao() : NobleCardModel(1, CardType.Attack, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DamagePerField", 4),
        ModCardVars.ComputedDamage("ZhangJiaoDamage",
            static ctx => ctx.BaseValue + ctx.GetCardBaseValueOrDefault("DamagePerField") *
                FgoField.ActiveOf(ctx.CombatState).Count, 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["ZhangJiaoDamage"].UpgradeValueBy(4);
        DynamicVars["DamagePerField"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("ZhangJiaoDamage"))
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}