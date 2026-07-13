using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class ExtraterrestrialOctopus() : FgoCardModel(2, CardType.Attack,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("StarMultiplier", 2),
        new ComputedDynamicVar("StarsDamage", 0, (card, target) => this.FgoRes().Stars * DynamicVars["StarMultiplier"].BaseValue)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["StarMultiplier"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, play.Target!,
            DynamicVars["StarsDamage"].BaseValue,
            ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
    }
}