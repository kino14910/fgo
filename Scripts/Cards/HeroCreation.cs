using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class HeroCreation : FgoCardModel
{
    public HeroCreation() : base(0, CardType.Skill,
        CardRarity.Common, TargetType.Self)
    {
        WithPower<StrengthPower>(2, 2);
        WithVar("CriticalDamage", 50, 50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        var strAmt = DynamicVars.Strength.BaseValue;
        var critAmt = DynamicVars["CriticalDamage"].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, self, strAmt, self, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, self, critAmt, self, this);
        await PowerCmd.Apply<TempCritDamagePower>(choiceContext, self, strAmt, self, this);
        var tempPower = self.GetPower<TempCritDamagePower>();
        if (tempPower != null) tempPower.CritDamageAmount = (int)critAmt;
    }
}