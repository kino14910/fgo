using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HeroCreation() : FgoCardModel(0, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<CriticalDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<HeroCreationTempStrengthPowerPower>(2),
        ModCardVars.Power<HeroCreationTempCritDamagePower>(50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(HeroCreationTempStrengthPowerPower)].UpgradeValueBy(2);
        DynamicVars[nameof(HeroCreationTempCritDamagePower)].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HeroCreationTempStrengthPowerPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(HeroCreationTempStrengthPowerPower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<HeroCreationTempCritDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(HeroCreationTempCritDamagePower)].BaseValue, Owner.Creature, this);
    }
}