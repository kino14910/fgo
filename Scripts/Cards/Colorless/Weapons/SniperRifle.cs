using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

/// <summary>狙击枪: 造成 8 点伤害；目标生命值低于其上限 50% 时再 +8。</summary>
/// <remarks>
///     <c>SniperDamage</c> 是**目标感知**的计算变量: 卡面拖到哪个敌人身上就按那个敌人的当前生命值换算，
///     实际结算用同一个求值器（<c>EvaluateValueOrDefault(target:)</c>），显示与结算不会漂移。
/// </remarks>
[RegisterCard(typeof(TokenCardPool))]
public class SniperRifle() : FgoBaseCardModel(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("LowHpBonus", 8),
        ModCardVars.ComputedDamage("SniperDamage", 8m, static (card, target) =>
        {
            var baseDamage = card?.DynamicVars["SniperDamage"].BaseValue ?? 8m;
            var bonus = card?.DynamicVars["LowHpBonus"].BaseValue ?? 0m;
            // 目标生命值低于上限的一半才加成（严格小于）。
            return target != null && target.CurrentHp * 2m < target.MaxHp ? baseDamage + bonus : baseDamage;
        })
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        await DamageCmd.Attack(DynamicVars.EvaluateValueOrDefault("SniperDamage", target: target))
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
