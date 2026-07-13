using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class StarHunter() : FgoCardModel(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Star", 8),
        new PowerVar<CriticalDamageOncePower>(50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(CriticalDamageOncePower)].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await FgoStarCmd.AddStars(DynamicVars["Star"].IntValue);
        await PowerCmd.Apply<CriticalDamageOncePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(CriticalDamageOncePower)].BaseValue, Owner.Creature, this);
    }
}