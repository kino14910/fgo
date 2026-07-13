using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class BridalSpinWheel(): NobleCardModel(1, CardType.Skill, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<PlatingPower>(2),
        ModCardVars.Int("Star", 8)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(PlatingPower)].UpgradeValueBy(1);
        DynamicVars["Star"].UpgradeValueBy(4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(PlatingPower)].BaseValue, Owner.Creature, this);
        await FgoStarCmd.AddStars(DynamicVars["Star"].IntValue);
    }
}