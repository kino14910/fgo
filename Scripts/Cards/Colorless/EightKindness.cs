using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards.Colorless;

public class EightKindness : FgoColorlessCard
{
    public EightKindness() : base(3, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithKeywords(CardKeyword.Exhaust);
        WithPower<StrengthPower>(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        var amount = DynamicVars.Strength.BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(StrengthPower).Name].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<RegenPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<ThornsPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<VigorPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, self, amount, Owner.Creature, this);
    }
}