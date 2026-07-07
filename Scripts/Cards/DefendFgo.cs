using Fgo.Scripts.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards;

[RegisterCharacterStarterCard(typeof(FgoCharacter), 4)]
public class DefendFgo : FgoCardModel
{
    public DefendFgo() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
    {
        WithBlock(6, 2);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
    }
}