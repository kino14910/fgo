using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class VoidSpaceFineArts() : FgoCardModel(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
{
    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RegenPower>(10),
        ModCardVars.Int("CurseStacks", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<RegenPower>(choiceContext, self, DynamicVars[nameof(RegenPower)].BaseValue, self,
            this);
        await PowerCmd.Apply<CursePower>(choiceContext, self, 3m, self, this);
        var curseCount = self.GetPowerAmount<CursePower>();
        await FgoNpCmd.AddNp((int)DynamicVars["CurseStacks"].BaseValue * curseCount);
    }
}