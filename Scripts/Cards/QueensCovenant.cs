using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class QueensCovenant() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<MyFairSoldierPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(2),
        ModCardVars.Power<DexterityPower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars[nameof(DexterityPower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var strAmount = DynamicVars[nameof(StrengthPower)].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(DexterityPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MyFairSoldierPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
    }
}