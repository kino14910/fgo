using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless;

public class EightKindness() : ModCardTemplate(3, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(1)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        var amount = DynamicVars.Strength.BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StrengthPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<RegenPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<ThornsPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<VigorPower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<IntangiblePower>(choiceContext, self, amount, Owner.Creature, this);
        await PowerCmd.Apply<ArtifactPower>(choiceContext, self, amount, Owner.Creature, this);
    }
}