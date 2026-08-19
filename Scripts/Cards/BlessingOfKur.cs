using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class BlessingOfKur() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(2),
        ModCardVars.Power<MaxHpPower>(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<MaxHpPower>(choiceContext, Owner.Creature, DynamicVars[nameof(MaxHpPower)].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<BlessingOfKurPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue,
            Owner.Creature, this);
    }
}