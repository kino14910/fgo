using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Cards.Colorless;

public class CurseDisaster() : ModCardTemplate(-2, CardType.Status,
    CardRarity.Status, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<CursePower>(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, DynamicVars[nameof(CursePower)].BaseValue,
            Owner.Creature, this);
        foreach (var enemy in CombatState!.HittableEnemies)
            await PowerCmd.Apply<CursePower>(choiceContext, enemy, DynamicVars[nameof(CursePower)].BaseValue,
                Owner.Creature, this);
    }
}