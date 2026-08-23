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
        HoverTipFactory.FromPower<HeroCreationPower>(),
        HoverTipFactory.FromPower<TempCritDamagePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<HeroCreationPower>(2),
        ModCardVars.Power<TempCritDamagePower>(50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(HeroCreationPower)].UpgradeValueBy(2);
        DynamicVars[nameof(TempCritDamagePower)].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<HeroCreationPower>(choiceContext, Owner.Creature, DynamicVars[nameof(HeroCreationPower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<TempCritDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(TempCritDamagePower)].BaseValue, Owner.Creature, this);
    }
}