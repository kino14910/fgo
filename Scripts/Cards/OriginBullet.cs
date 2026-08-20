using Fgo.Scripts.Keywords;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class OriginBullet() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FgoKeywords.IgnoreInvincible, CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var amount = await IgnoreInvincibleAction(cardPlay.Target);
        await PowerCmd.Apply<IgnoreInvinciblePower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
    }
}