using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class StarHunter() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalDamageOncePower>(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Star", 8),
        ModCardVars.Power<CriticalDamageOncePower>(50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(CriticalDamageOncePower)].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await FgoResCmd.ModifyStars(DynamicVars["Star"].BaseValue, Owner);
        var power = await PowerCmd.Apply<CriticalDamageOncePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(CriticalDamageOncePower)].BaseValue, Owner.Creature, this);
        if (power != null)
            power.Amount2 = 3;
    }
}