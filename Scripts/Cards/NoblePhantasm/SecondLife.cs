using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class SecondLife(): NobleCardModel(1, CardType.Skill, TargetType.Self) {

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        ModCardVars.Int("Np", 20)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var exhaustCards = Owner.PlayerCombatState!.ExhaustPile.Cards.ToList();
        if (exhaustCards.Count == 0) return;

        var card = exhaustCards[Random.Shared.Next(exhaustCards.Count)];
        var copy = card.ToMutable();
        if (copy.IsUpgradable)
            CardCmd.Upgrade(copy, CardPreviewStyle.None);
        await FgoCardActions.AddToPile(copy, PileType.Hand);

        foreach (var enemy in CombatState!.HittableEnemies)
            if (enemy.HasPower<MinionPower>())
            {
                await CreatureCmd.Kill(enemy);
                await FgoNpCmd.AddNp(DynamicVars["Np"].IntValue);
            }
    }
}