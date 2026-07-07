using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class FacelessMoon : FgoCardModel
{
    public FacelessMoon() : base(1, CardType.Skill,
        CardRarity.Common, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithBlock(5, 3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        // Retain + block per retained handled by FacelessMoonPower
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }
}