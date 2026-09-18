using Fgo.Scripts.Fields;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     桥之巨人（Thames Troll）: 造成伤害并获得格挡；
///     在〔森林〕〔都市〕〔黑暗〕场地上时，再来同样的一套（第二段伤害与格挡）。
/// </summary>
/// <remarks>
///     追加效果是**独立的第二段**伤害与格挡，不是把数值合并进第一段——
///     因此会各触发一次「受到伤害/造成伤害」类效果（反伤、暴击星等也会分别结算）。
/// </remarks>
public class ThamesTroll() : FgoCardModel(3, CardType.Attack, 
    CardRarity.Rare, TargetType.AnyEnemy)
{
    /// <summary>触发追加段的场地（任一存在即可）。</summary>
    private static readonly FgoFieldId[] BonusFields =
        [FgoFieldId.Forest, FgoFieldId.City, FgoFieldId.Darkness];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoFieldId.Forest.ToHoverTip(),
        FgoFieldId.City.ToHoverTip(),
        FgoFieldId.Darkness.ToHoverTip(),
        HoverTipFactory.FromPower<BlindPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Damage(15),
        ModCardVars.Block(15),
        ModCardVars.Int("FieldDamage", 6),
        ModCardVars.Int("FieldBlock", 6)
    ];

    public override bool GainsBlock => true;

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["FieldDamage"].UpgradeValueBy(6);
        DynamicVars["FieldBlock"].UpgradeValueBy(6);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await Strike(choiceContext, cardPlay, cardPlay.Target, DynamicVars.Damage.BaseValue,
            DynamicVars.Block.BaseValue);

        if (!OnBonusField()) return;

        await Strike(choiceContext, cardPlay, cardPlay.Target, DynamicVars["FieldDamage"].BaseValue,
            DynamicVars["FieldBlock"].BaseValue);
    }

    /// <summary>「伤害 + 格挡」这一套结算，本卡会走一到两遍。</summary>
    private async Task Strike(PlayerChoiceContext choiceContext, CardPlay cardPlay, Creature target,
        decimal damage, decimal block)
    {
        await DamageCmd.Attack(damage)
            .FromCard(this, cardPlay)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, cardPlay);
    }

    private bool OnBonusField()
    {
        var combat = Owner.Creature.CombatState;
        return combat != null && BonusFields.Any(id => FgoField.Has(combat, id));
    }
}
