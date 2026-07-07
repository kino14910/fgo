using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class AnimalDialogue : FgoCardModel
{
    public AnimalDialogue() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("Threshold", 20);
    }

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