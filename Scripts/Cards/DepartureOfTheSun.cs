using Fgo.Scripts.Powers;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class DepartureOfTheSun() : FgoCardModel(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedPowerAmountGiven<CriticalDamagePower>(
            ctx => this.FgoRes().Stars / 10 * ctx.BaseValue, 20)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["CriticalDamagePower"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
            DynamicVars.EvaluateValueOrDefault("CriticalDamagePower"), Owner.Creature, this);
    }
}