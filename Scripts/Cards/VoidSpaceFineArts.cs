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

public class VoidSpaceFineArts() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>(),
        HoverTipFactory.FromPower<CursePower>()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(10),
        ModCardVars.Int("CurseStacks", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<GutsPower>(choiceContext, self, DynamicVars[nameof(GutsPower)].BaseValue, self,
            this);
        for (var i = 0; i < 3; i++) await PowerCmd.Apply<CursePower>(choiceContext, self, 1m, self, this);

        var curseCount = self.GetPowerAmount<CursePower>();
        await FgoResCmd.ModifyNp(DynamicVars["CurseStacks"].BaseValue * curseCount, play.Player);
    }
}