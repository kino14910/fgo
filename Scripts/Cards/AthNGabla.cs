using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AthNGabla() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(5),
        ModCardVars.Computed("StrengthGain", 2, (card, _) =>
            card?.Owner.Creature is { } c
                ? 2 + Math.Max(0, (c.MaxHp - c.CurrentHp) / 20)
                : 2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(GutsPower)].UpgradeValueBy(8);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GutsPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(GutsPower)].BaseValue, Owner.Creature, this);

        var strength = (int)DynamicVars.EvaluateValueOrDefault("StrengthGain", target: Owner.Creature);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strength, Owner.Creature, this);
    }
}