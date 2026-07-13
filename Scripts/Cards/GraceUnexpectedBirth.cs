using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class GraceUnexpectedBirth() : FgoCardModel(0, CardType.Skill,
        CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SealNpPower>(1),
        ModCardVars.Int("Np", 30)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
        await PowerCmd.Apply<SealNpPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(SealNpPower)].BaseValue, Owner.Creature, this);
    }
}