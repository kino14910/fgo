using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class AntiBeastMagecraft : FgoCardModel
{
    public AntiBeastMagecraft() : base(1, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithPower<AntiBeastMagecraftPower>(3, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<AntiBeastMagecraftPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(AntiBeastMagecraftPower).Name].BaseValue, Owner.Creature, this);
    }
}