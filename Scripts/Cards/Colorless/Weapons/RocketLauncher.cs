using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

/// <summary>火箭筒: 对主目标造成 12 点伤害，对**其他**敌人各造成 6 点溅射伤害。</summary>
[RegisterCard(typeof(TokenCardPool))]
public class RocketLauncher() : FgoBaseCardModel(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(12),
        ModCardVars.Int("Splash", 6)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null) return;

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_fire_burst")
            .Execute(choiceContext);

        // 溅射目标在主体伤害落地后再取一次: 已被打死的敌人不该再吃溅射。
        // 溅射走 CreatureCmd.Damage 而不是再开一条 AttackCommand —— "对其他敌人各 6 点"是固定值，
        // 不该再被暴击/攻击钩子二次放大。
        var others = combat.HittableEnemies.Where(enemy => enemy != target && enemy.IsAlive).ToList();
        if (others.Count == 0) return;

        await CreatureCmd.Damage(choiceContext, others, DynamicVars["Splash"].BaseValue,
            ValueProp.Move, Owner.Creature, this, cardPlay);
    }
}
