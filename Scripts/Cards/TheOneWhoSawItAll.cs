using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class TheOneWhoSawItAll() : FgoCardModel(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DamageBoost", 3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["DamageBoost"].UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var attack = Owner.PlayerCombatState!.DrawPile.Cards
            .Where(card => card.Type == CardType.Attack)
            .OrderBy(_ => Random.Shared.Next())
            .FirstOrDefault();
        if (attack == null) return;

        FgoCardActions.BoostDamage(attack, DynamicVars["DamageBoost"].IntValue);
        await CardPileCmd.Add(attack, PileType.Hand, CardPilePosition.Top, this, true);
    }
}