using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

/// <summary>
///     穿刺之雷刃（Skewered Plasma Blade）: 造成 32 点伤害，以 {StunChance}% 概率给予眩晕（升级 +10%）。
/// </summary>
public class SkeweredPlasmaBlade() : NobleCardModel(1, CardType.Attack, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(32),
        ModCardVars.Int("StunChance", 60)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["StunChance"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // 60%（升级 70%）概率给予眩晕。
        var roll = Owner.RunState.Rng.Niche.NextFloat() * 100f;
        if (roll < (float)DynamicVars["StunChance"].BaseValue)
            await CreatureCmd.Stun(cardPlay.Target);
    }
}