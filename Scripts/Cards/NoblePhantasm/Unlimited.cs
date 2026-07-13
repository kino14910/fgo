using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class Unlimited(): NobleCardModel(1, CardType.Power, TargetType.Self) {

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("AttacksPerTurn", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<UnlimitedPower>(choiceContext, Owner.Creature, DynamicVars["AttacksPerTurn"].BaseValue,
            Owner.Creature, this);
    }
}