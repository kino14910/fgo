using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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

        if (cardPlay.Target.Monster is not { IntendsToAttack: true } monster)
            return;

        var total = monster.NextMove.Intents.OfType<AttackIntent>()
            .Sum(a =>
            {
                var raw = a.DamageCalc?.Invoke() ?? 0m;
                var hit = Hook.ModifyDamage(cardPlay.Player.RunState, cardPlay.Player.Creature.CombatState,
                    cardPlay.Player.Creature, monster.Creature, raw, ValueProp.Move,
                    null, null, ModifyDamageHookType.All, CardPreviewMode.None, out _);
                return Math.Max(0m, hit) * Math.Max(1, a.Repeats);
            });
        await DamageCmd.Attack(total)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }
}