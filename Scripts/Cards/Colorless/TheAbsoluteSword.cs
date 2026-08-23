using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(TokenCardPool))]
public class TheAbsoluteSword() : FgoBaseCardModel(3, CardType.Attack,
    CardRarity.Token, TargetType.AllEnemies)
{
    private const int DamageThreshold = 80;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(32)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 通过 AttackCommand.Results 直接读取本次伤害，无需施加追踪能力
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_slash");
        await attack.Execute(choiceContext);

        var totalDamage = attack.Results
            .SelectMany(hit => hit)
            .Sum(r => r.TotalDamage);

        // 总伤害 >= 阈值，再次造成等量伤害（翻倍）
        if (totalDamage >= DamageThreshold)
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
    }
}