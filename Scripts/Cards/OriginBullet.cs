using System.Collections.Generic;
using System.Threading.Tasks;
using Fgo.Scripts.Keywords;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class OriginBullet() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [FgoKeywords.IgnoreInvincible, CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if(play.Target is null) return;
        var amount = await IgnoreInvincibleAction(play);
        await PowerCmd.Apply<IgnoreInvinciblePower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
    }
}