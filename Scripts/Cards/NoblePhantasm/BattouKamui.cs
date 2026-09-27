using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BattouKamui() : NobleCardModel(1, CardType.Attack, TargetType.AnyEnemy)
{
    /// <summary>
    ///     宝具默认不参与暴击（暴击只作用于普通攻击牌），本卡是例外：
    ///     与攻击牌一样消耗 10 颗暴击星并把这段「失去生命」翻倍。
    /// </summary>
    public override bool CanCrit => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        FgoHoverTipFactory.FromStar()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<BattouKamuiPower>(6),
        // Unblockable = 失去生命（无视格挡）；不带 Unpowered ⇒ 仍然受能力与暴击倍率影响。
        ModCardVars.Damage(36m, ValueProp.Unblockable | ValueProp.Move)
    ];

    protected override bool ShouldGlowGoldInternal =>
        IsMutable && Owner is not null && FgoBattleHooks.Get(Owner).CanCrit;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 先加临时力量：本次「失去生命」当回合就吃到这 6 点加成。
        await PowerCmd.Apply<BattouKamuiPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(BattouKamuiPower)].BaseValue,
            Owner.Creature, this);

        // 本卡不走 AttackCommand（要 Unblockable），因此不会触发 BeforeAttack 里的暴击判定，
        // 需要在这里手动开启：消耗暴击星后 ModifyDamageMultiplicative 才会返回暴击倍率。
        await FgoBattleHooks.Get(Owner).TryActivateCrit(this);

        await CreatureCmd.Damage(choiceContext, cardPlay.Target,
            DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Move,
            Owner.Creature, this, cardPlay);
    }
}
