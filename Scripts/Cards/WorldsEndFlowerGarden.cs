using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class WorldsEndFlowerGarden : FgoCardModel
{
    public WorldsEndFlowerGarden() : base(2, CardType.Power,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithVar("NPOnCrit", 10, 5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<WorldsEndFlowerGardenPower>(choiceContext, Owner.Creature,
            DynamicVars["NPOnCrit"].BaseValue, Owner.Creature, this);
    }
}