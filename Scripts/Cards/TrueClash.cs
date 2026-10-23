using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class TrueClash() : FgoCardModel(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Block(8)
    ];

    protected override bool ShouldGlowGoldInternal => 
        CombatState != null &&
        CombatState.HittableEnemies.Any(e =>
            e.Monster?.IntendsToAttack ?? false);

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        if (!cardPlay.Target.Monster!.IntendsToAttack)
            return;
        var monster = cardPlay.Target.Monster;
        var intents = monster?.NextMove.Intents ?? [];

        var attackIntents = intents.OfType<AttackIntent>().ToList();
        var total = attackIntents.Sum(a => a.GetTotalDamage(monster!.CombatState.Allies, monster.Creature));

        await DamageCmd.Attack(total)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }
}