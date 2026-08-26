using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(TokenCardPool))]
public class InfantileRegression() : FgoBaseCardModel(0, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Energy(1),
        ModCardVars.Int("Np", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

        var maxHpPower = Owner.Creature.GetPower<MaxHpPower>();
        if (maxHpPower == null) return;

        var extraHp = maxHpPower.Amount;

        await PowerCmd.Remove<MaxHpPower>(Owner.Creature);

        var pairs = extraHp / 9;
        if (pairs <= 0) return;
        await FgoResCmd.ModifyNp((int)(pairs * DynamicVars["Np"].BaseValue), Owner);
    }
}