using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class VoidSpaceFineArts : FgoCardModel
{
    public VoidSpaceFineArts() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithPower<RegenPower>(10);
        WithVar("CurseStacks", 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<RegenPower>(choiceContext, self, DynamicVars[typeof(RegenPower).Name].BaseValue, self,
            this);
        await PowerCmd.Apply<CursePower>(choiceContext, self, 3m, self, this);
        var curseCount = self.GetPowerAmount<CursePower>();
        await FgoNpCmd.AddNp((int)DynamicVars["CurseStacks"].BaseValue * curseCount);
    }
}