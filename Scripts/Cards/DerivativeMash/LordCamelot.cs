using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     已然遥远的理想之城: Camelot(拟似展开／人理之础)升级获得
/// </summary>
public class LordCamelot() : NobleCardModel(0, CardType.Power, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<ReducePercentDamagePower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<PlatingPower>(1),
        ModCardVars.Power<ReducePercentDamagePower>(30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(1);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 全体获得：给玩家方同队所有生物（含自己）施加；StrengthPower 只给除自己以外的队友。
        var allies = CombatState!.GetTeammatesOf(Owner.Creature);
        await PowerCmd.Apply<PlatingPower>(choiceContext, allies,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, allies,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, allies.Where(c => c != Owner.Creature),
            3m, Owner.Creature, this);
    }
}