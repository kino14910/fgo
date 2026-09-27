using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

/// <summary>
///     掎角一阵（Kikaku Ichijin）: 多人专用宝具。指定一名其他友方献祭，
///     以牺牲者的最大生命值为基准对所有敌人造成伤害。
/// </summary>
/// <remarks>
///     献祭走 <c>CreatureCmd.Kill</c> 的**非强制**路径（`force: false`）——它会先问
///     <c>Hook.ShouldDie</c>，有阻止者时落到 <c>AfterPreventingDeath</c>，
///     所以〔毅力〕与令咒的救人逻辑照常生效，救回来了伤害也照打。
///     <para />
///     伤害值在击杀**之前**取: 死亡会走 <c>RemoveAllPowersAfterDeath</c>，
///     额外最大生命值这类能力被移除时会把 MaxHp 还原，之后读就偏小了。
/// </remarks>
public class KikakuIchijin() : NobleCardModel(1, CardType.Skill, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Percent", 100),
        ModCardVars.ComputedDamage("KikakuDamage", 0m, static (card, target) =>
        {
            var percent = card?.DynamicVars["Percent"].BaseValue ?? 100m;
            return (target?.MaxHp ?? 0m) * percent / 100m;
        })
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Percent"].UpgradeValueBy(25);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var sacrifice = cardPlay.Target;
        if (sacrifice == null) return;

        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var damage = DynamicVars.EvaluateValueOrDefault("KikakuDamage", target: sacrifice);

        await CreatureCmd.Kill(sacrifice);

        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combat)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
