using Fgo.Scripts.Character;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards;

[RegisterCharacterStarterCard(typeof(FgoCharacter))]
public class DreamUponTheStars : FgoCardModel
{
    public DreamUponTheStars() : base(2, CardType.Skill,
        CardRarity.Basic, TargetType.Self)
    {
        WithBlock(8, 3);
        WithPower<NpDamagePower>(20);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
    }
}