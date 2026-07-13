using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class KnightOfTheLake() : FgoCardModel(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("CritDamage", 30),
        ModCardVars.Int("Stars", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["CritDamage"].UpgradeValueBy(20);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!Owner.Creature.HasPower<CriticalDamagePower>())
            await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
                DynamicVars["CritDamage"].BaseValue, Owner.Creature, this);
        else
            await FgoStarCmd.AddStars(DynamicVars["Stars"].IntValue);
    }
}