using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.DerivativeMash;

/// <summary>
///     已然遥远的理想之城: Camelot(拟似展开／人理之础)升级获得
/// </summary>
public class LordCamelot() : NobleCardModel(0, CardType.Power, TargetType.Self)
{
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
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<ReducePercentDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(ReducePercentDamagePower)].BaseValue, Owner.Creature, this);
    }
}