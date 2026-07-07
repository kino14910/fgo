using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class MahaPralaya : NobleCardModel
{
    public MahaPralaya() : base(3, CardType.Attack, TargetType.Self)
    {
        WithDamage(8, 3);
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

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}