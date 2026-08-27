using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

/// <summary>
///     闪烁于终局宇宙的兽冠（Edin Shugurra Collapsar）: 获得好感度，按好感档位触发强化效果，
///     对所有敌人造成伤害并给予灾厄。
/// </summary>
public class EdinShugurraCollapsar() : NobleCardModel(1, CardType.Skill, TargetType.AllEnemies)
{
    private const int ArtifactThreshold = 40;
    private const int BlockThreshold = 70;
    private const int StarThreshold = 100;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<DoomPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(24),
        ModCardVars.Int("Affection", 0),
        ModCardVars.Power<DoomPower>(12)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
        DynamicVars["Affection"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先获得好感度，再按新值判定档位效果。
        FgoBattleHooks.Get(Owner).ModifyAffection((int)DynamicVars["Affection"].BaseValue, Owner);
        var affection = FgoBattleHooks.Get(Owner).Affection;

        // 好感度 40 以上: 去除所有敌人的人工制品。
        if (affection >= ArtifactThreshold)
            foreach (var enemy in CombatState!.HittableEnemies
                         .Where(e => e.HasPower<ArtifactPower>())
                         .ToList())
                await PowerCmd.Remove<ArtifactPower>(enemy);

        // 好感度 70 以上: 去除所有敌人的格挡。
        if (affection >= BlockThreshold)
            foreach (var enemy in CombatState!.HittableEnemies
                         .Where(e => e.Block > 0)
                         .ToList())
                await CreatureCmd.LoseBlock(choiceContext, enemy, enemy.Block, Owner.Creature);

        // 好感度 100: 获得 50 暴击星。
        if (affection >= StarThreshold)
            await FgoResCmd.ModifyStars(50, Owner);

        // 对所有敌人造成伤害，每 10 层好感度额外 +3。
        var bonus = affection / 10 * 3;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        // 给予所有敌人 12 层灾厄。
        await PowerCmd.Apply<DoomPower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(DoomPower)].BaseValue, Owner.Creature, this);
    }
}