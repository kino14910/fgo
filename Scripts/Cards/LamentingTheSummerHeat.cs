using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

/// <summary>
///     对空虚酷暑的叹息（Lamenting the Summer Heat）: 失去 1 点生命，给予 2(3) 层虚弱。
/// </summary>
public class LamentingTheSummerHeat() : FgoCardModel(1, CardType.Skill,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<WeakPower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(WeakPower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        // 失去 1 点生命（不可格挡、不受力量加成）。
        await CreatureCmd.Damage(choiceContext, Owner.Creature, 1m,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);

        // 给予目标 {WeakPower} 层虚弱。
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,
            DynamicVars[nameof(WeakPower)].BaseValue, Owner.Creature, this);
    }
}