using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HolyShroud() : FgoCardModel(0, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    /// <summary>
    ///     触发阈值：怪物本回合意图（攻击 / 即死）将要对<b>卡牌拥有者</b>造成的总伤害达到该值时生效。
    /// </summary>
    private const decimal IncomingDamageThreshold = 12m;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<ReducePercentDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];

    protected override bool ShouldGlowGoldInternal =>
        IncomingDamageForOwner() >= IncomingDamageThreshold;

    protected override bool ShouldGlowRedInternal => !ShouldGlowGoldInternal;

    private decimal IncomingDamageForOwner()
    {
        if (CombatState is not { } combatState) return 0m;
        if (Owner.Creature is not { } owner) return 0m;

        var total = 0m;
        foreach (var enemy in combatState.Enemies)
        {
            if (!enemy.IsAlive) continue;
            if (enemy.Monster is not { } monster) continue;
            if (monster.NextMove is not { } nextMove) continue;

            foreach (var intent in nextMove.Intents.OfType<AttackIntent>())
            {
                if (intent.IntentType is not (IntentType.Attack or IntentType.DeathBlow)) continue;
                if (intent.DamageCalc is not { } damageCalc) continue;

                var perHit = Hook.ModifyDamage(
                    Owner.RunState,
                    combatState,
                    owner,
                    enemy,
                    damageCalc(),
                    ValueProp.Move,
                    null,
                    null,
                    ModifyDamageHookType.All,
                    CardPreviewMode.None,
                    out _);

                total += Math.Max(0m, perHit) * Math.Max(1, intent.Repeats);
            }
        }

        return total;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (ShouldGlowGoldInternal)
            await PowerCmd.Apply<ReducePercentDamagePower>(
                choiceContext,
                Owner.Creature,
                DynamicVars[nameof(ReducePercentDamagePower)].BaseValue,
                Owner.Creature,
                this);
    }
}