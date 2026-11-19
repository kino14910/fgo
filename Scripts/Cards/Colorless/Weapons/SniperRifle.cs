using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

[RegisterCard(typeof(TokenCardPool))]
public class SniperRifle() : FgoBaseCardModel(0, CardType.Attack,
    CardRarity.Token, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage("SniperDamage", 6m, static (card, target) =>
        {
            var baseDamage = card?.DynamicVars["SniperDamage"].BaseValue ?? 6m;
            return target != null && target.CurrentHp * 2m <= target.MaxHp ? baseDamage * 2 : baseDamage;
        })
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["SniperDamage"].UpgradeValueBy(2);
    }

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