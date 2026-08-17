using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ExtraterrestrialOctopus() : FgoCardModel(0, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("StarMultiplier", 2),
        ModCardVars.Computed("StarsDamage", context =>
            ModelDb.Singleton<FgoPlayerResources>().Stars * context.GetCardBaseValueOrDefault("StarMultiplier"), 0)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["StarMultiplier"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, play.Target!,
            DynamicVars.EvaluateValueOrDefault("StarsDamage"),
            ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
    }
}