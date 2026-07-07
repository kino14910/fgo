using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class ExtraterrestrialOctopus : FgoCardModel
{
    public ExtraterrestrialOctopus() : base(2, CardType.Attack,
        CardRarity.Uncommon, TargetType.AnyEnemy)
    {
        WithTags(FgoTags.Foreigner);
        WithVar("StarMultiplier", 2, 1);
        WithCalculatedDamage(0, static (card, _) =>
        {
            var stars = card.FgoRes().Stars;
            var multiplier = card.DynamicVars["StarMultiplier"].IntValue;
            var totalDamage = stars * multiplier;
            return totalDamage;
        });
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.Damage(choiceContext, play.Target!, DynamicVars.CalculatedDamage.BaseValue,
            ValueProp.Unpowered | ValueProp.Move, Owner.Creature);
    }
}