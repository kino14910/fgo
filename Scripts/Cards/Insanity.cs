using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class Insanity() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<InsanityPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<InsanityPower>(10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["InsanityPower"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<InsanityPower>(choiceContext, Owner.Creature, DynamicVars["InsanityPower"].IntValue,
            Owner.Creature, this);
    }
}