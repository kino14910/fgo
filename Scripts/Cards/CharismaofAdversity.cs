using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CharismaOfAdversity() : FgoCardModel(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(6),
        // 计算命中次数: 基础 1 次，每失去 6 点生命额外 +1 次。
        ModCardVars.Computed("CalculatedHits", 0, (card, _) =>
            card.Owner?.Creature is { } c
                ? 1 + Math.Max(0, (c.MaxHp - c.CurrentHp) / 6)
                : 0)
    ];

    protected override bool ShouldGlowGoldInternal =>
        Owner.Creature.MaxHp - Owner.Creature.CurrentHp >= 12;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var hits = (int)DynamicVars.EvaluateValueOrDefault("CalculatedHits", target: cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitCount(hits)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}