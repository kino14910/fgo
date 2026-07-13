using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AnimalDialogue() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Threshold", 20)
    ];

    protected override bool ShouldGlowGoldInternal =>
        this.FgoRes().Np >= DynamicVars["Threshold"].IntValue;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var np = this.FgoRes().Np;
        var threshold = DynamicVars["Threshold"].IntValue;
        if (np >= threshold)
        {
            var energyGain = np / threshold;
            await PlayerCmd.GainEnergy(energyGain, Owner);
            await FgoNpCmd.ResetNp();
        }
    }
}