using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class MahaPralaya(): NobleCardModel(3, CardType.Attack, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(8, ValueProp.Move)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var debuffTypes = CombatState!.HittableEnemies
            .SelectMany(enemy => enemy.Powers)
            .Where(power => power.Type == PowerType.Debuff)
            .Select(power => power.Id)
            .Distinct()
            .Count();

        var hitCount = Math.Max(1, debuffTypes);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitCount(hitCount)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}