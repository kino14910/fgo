using Fgo.Scripts.Fields;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

/// <summary>
///     夜间露天咖啡座（Caféterras bij nacht）: 对所有敌人造成伤害，按概率付与〔眩晕〕，
///     并让战场进入〔都市〕。
/// </summary>
/// <remarks>
///     概率走 <c>Owner.RunState.Rng.Niche</c>，与 <see cref="SkeweredPlasmaBlade" /> 同一条通道。
///     OnPlay 处于被复制的游戏动作内，各端按同一随机序列推进，因此不需要额外的跨端同步。
///     <para />
///     〔都市〕的效果尚未确定（见 <c>FgoFieldEffects</c> 的 TODO），目前只施加场地、不做结算。
/// </remarks>
public class CafeterrasBijNacht() : NobleCardModel(2, CardType.Attack, TargetType.AllEnemies)
{
    /// <summary>〔眩晕〕的付与概率（百分比）。</summary>
    public const int StunChancePercent = 40;

    /// <summary>施加的〔都市〕持续回合数。</summary>
    public const int CityTurns = 3;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.City.ToHoverTip(),
        HoverTipFactory.Static(StaticHoverTip.Stun)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(32),
        ModCardVars.Int("StunChance", StunChancePercent)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(combat)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 伤害落地后再取一次目标: 被打死的敌人不该再被眩晕。
        // 概率当场只掷一次（"40% 概率付与所有敌方"），不是每个敌人各掷一次。
        var enemies = combat.HittableEnemies;
        var roll = Owner.RunState.Rng.Niche.NextFloat() * 100f;
        if (roll < (float)DynamicVars["StunChance"].BaseValue)
            foreach (var enemy in enemies)
                await CreatureCmd.Stun(enemy);

        await FgoField.Add(combat, FgoFieldId.City, CityTurns);
    }
}
