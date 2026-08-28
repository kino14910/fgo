using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Keywords;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     印证希望的人理之剑: LordChaldeas(构筑希望的人理之盾)发动后获得
/// </summary>
public class RayProofKyrielight() : NobleCardModel(1, CardType.Attack, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FgoKeywords.IgnoreInvincible];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(30),
        ModCardVars.Power<VulnerablePower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
        DynamicVars[nameof(VulnerablePower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await IgnoreInvincibleAction(CombatState!.HittableEnemies);

        await PowerCmd.Apply<VulnerablePower>(choiceContext, CombatState!.HittableEnemies,
            DynamicVars[nameof(VulnerablePower)].BaseValue, Owner.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);

        IEnumerable<Creature> targets = CombatState.HittableEnemies;
        if (CombatState.Encounter?.RoomType == RoomType.Boss)
            targets = targets.Where(c => c.IsSecondaryEnemy);
        foreach (var enemy in targets)
        {
            var buffs = enemy.Powers.Where(power => power.Type == PowerType.Buff).ToList();
            foreach (var buff in buffs)
                await PowerCmd.Remove(buff);
        }
    }
}