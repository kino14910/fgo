using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AlongSide() : FgoCardModel(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Computed("CalculatedBlock", 0, (card, _) =>
            card.Owner.Creature is { } c
                ? Math.Max(0, c.MaxHp - c.CurrentHp)
                : 0)
    ];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var block = (int)DynamicVars.EvaluateValueOrDefault("CalculatedBlock", target: cardPlay.Target);
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, cardPlay);
    }
}