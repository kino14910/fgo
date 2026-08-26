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
///     拟似展开／人理之础: 马修初始宝具卡。
/// </summary>
public class Camelot() : NobleCardModel(0, CardType.Power, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PlatingPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<PlatingPower>(1),
        ModCardVars.Power<ReducePercentDamagePower>(20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(1);
        DynamicVars[nameof(ReducePercentDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 全体获得：给玩家方同队所有生物（含自己）施加。
        var allies = CombatState!.GetTeammatesOf(Owner.Creature);
        await PowerCmd.Apply<PlatingPower>(choiceContext, allies,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, allies,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}