using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class TheOneWhoSawItAll() : FgoCardModel(0, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("DamageBoost", 6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["DamageBoost"].UpgradeValueBy(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attack = PileType.Draw.GetPile(Owner).Cards.Where(card => card.Type == CardType.Attack)
            .TakeRandom(1, Owner.RunState.Rng.CombatCardSelection).FirstOrDefault();

        if (attack == null) return;

        FgoCardActions.BoostDamage(attack, DynamicVars["DamageBoost"].IntValue);
        await CardPileCmd.Add(attack, PileType.Hand, CardPilePosition.Top, this);
    }
}