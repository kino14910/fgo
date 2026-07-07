using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class Insanity : FgoCardModel
{
    public Insanity() : base(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithTags(FgoTags.Foreigner);
        WithVar("NPOnCurseRemove", 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<InsanityPower>(choiceContext, Owner.Creature, DynamicVars["NPOnCurseRemove"].BaseValue,
            Owner.Creature, this);
    }
}