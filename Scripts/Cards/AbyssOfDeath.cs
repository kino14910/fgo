using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AbyssOfDeath() : FgoCardModel(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(10),
        ModCardVars.Power<DeathOfDeathPower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(GutsPower)].UpgradeValueBy(15);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(GutsPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DeathOfDeathPower>(choiceContext, Owner.Creature,
            1, Owner.Creature, this);
    }
}