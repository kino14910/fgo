using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class EightKindness() : FgoCardModel(2, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.FromPower<PlatingPower>(),
        HoverTipFactory.FromPower<RegenPower>(),
        HoverTipFactory.FromPower<ThornsPower>(),
        HoverTipFactory.FromPower<VigorPower>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
        HoverTipFactory.FromPower<NpRatePower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<StrengthPower>(1)
    ];

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var self = Owner.Creature;
        var amount = DynamicVars.Strength.BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<RegenPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<ThornsPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<VigorPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<NpRatePower>(choiceContext, self, amount, Owner.Creature, this);
    }
}