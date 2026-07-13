using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class RayHorizon(): NobleCardModel(0, CardType.Skill, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Np", 50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        FgoCardActions.ForceNextNpCard(Owner);
        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue, choiceContext, Owner);
        await PowerCmd.Apply<InvincibilityTurnPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}