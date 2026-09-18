using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class RoadOfFallingFlowers() : FgoCardModel(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamagePower>(),
        FgoHoverTipFactory.FromStar(),
        FgoHoverTipFactory.FromNp()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Star", 5),
        ModCardVars.Int("Np", 10),
        ModCardVars.Int("HealPercent", 30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Star"].UpgradeValueBy(5);
        DynamicVars["Np"].UpgradeValueBy(10);
        DynamicVars["HealPercent"].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FgoResCmd.ModifyStars(DynamicVars["Star"].BaseValue, Owner);
        await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, Owner);

        var power = await PowerCmd.Apply<RoadOfFallingFlowersPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (power != null)
            power.HealBonus = DynamicVars["HealPercent"].BaseValue / 100m;
    }
}