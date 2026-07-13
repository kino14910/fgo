using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class HeroCreation() : FgoCardModel(0, CardType.Skill,
        CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2),
        ModCardVars.Int("CriticalDamage", 50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(2);
        DynamicVars["CriticalDamage"].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        var strAmt = DynamicVars[nameof(StrengthPower)].BaseValue;
        var critAmt = DynamicVars["CriticalDamage"].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, self, strAmt, self, this);
        await PowerCmd.Apply<CriticalDamagePower>(choiceContext, self, critAmt, self, this);
        await PowerCmd.Apply<TempCritDamagePower>(choiceContext, self, strAmt, self, this);
        var tempPower = self.GetPower<TempCritDamagePower>();
        if (tempPower != null) tempPower.CritDamageAmount = (int)critAmt;
    }
}