using Fgo.Scripts.Commands;
using Fgo.Scripts.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class EdinShugurraCollapsar() : NobleCardModel(1, CardType.Attack, TargetType.AllEnemies)
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
        var ii = Owner.GetRelic<II>();
        ii?.ModifyAffection(DynamicVars["Affection"].IntValue);
        var affection = ii?.Affection ?? 0;

        if (affection >= ArtifactThreshold)
            foreach (var enemy in CombatState!.HittableEnemies
                         .Where(e => e.HasPower<ArtifactPower>())
                         .ToList())
                await PowerCmd.Remove<ArtifactPower>(enemy);

        if (affection >= BlockThreshold)
            foreach (var enemy in CombatState!.HittableEnemies
                         .Where(e => e.Block > 0)
                         .ToList())
                await CreatureCmd.LoseBlock(choiceContext, enemy, enemy.Block, Owner.Creature);

        if (affection >= StarThreshold)
            await FgoResCmd.ModifyStars(50, Owner);

        var bonus = affection / 10 * 3;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        await PowerCmd.Apply<DoomPower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(DoomPower)].BaseValue, Owner.Creature, this);
    }
}