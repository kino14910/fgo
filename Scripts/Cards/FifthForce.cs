using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class FifthForce() : FgoCardModel(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    private FifthForcePower? _fifthForcePower;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FifthForcePower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<FifthForcePower>(1),
        ModCardVars.Int("DamageReduction", 50)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<FifthForcePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(FifthForcePower)].BaseValue,
            Owner.Creature, this);
        if (power == null) return;
        power.DynamicVars["DamageReduction"].BaseValue = 50m;
        _fifthForcePower = power;
    }

    protected override void OnUpgrade()
    {
        _fifthForcePower?.DynamicVars["DamageReduction"].UpgradeValueBy(-15m);
        DynamicVars["DamageReduction"].UpgradeValueBy(-15m);
    }
}