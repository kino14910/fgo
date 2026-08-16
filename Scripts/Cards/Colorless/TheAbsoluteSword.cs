using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.Colorless;

public class TheAbsoluteSword() : FgoColorlessCardModel(3, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies)
{
    private const int DamageThreshold = 80;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(32)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // 通过 AttackCommand.Results 直接读取本次伤害，无需施加追踪能力
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash");
        await attack.Execute(choiceContext);

        var totalDamage = attack.Results
            .SelectMany(hit => hit)
            .Sum(r => r.TotalDamage);

        // 总伤害 >= 阈值，再次造成等量伤害（翻倍）
        if (totalDamage >= DamageThreshold)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, play)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
    }
}