using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class Indomitable() : FgoCardModel(1, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NonStackableGutsPower>(),
        HoverTipFactory.FromPower<IndomitablePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NonStackableGutsPower>(5),
        ModCardVars.Power<IndomitablePower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NonStackableGutsPower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NonStackableGutsPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<IndomitablePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(IndomitablePower)].BaseValue, Owner.Creature, this);
    }
}