using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CrownedWithLife() : FgoCardModel(-2, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Tokens", 2),
        new PowerVar<NonStackableGutsPower>(30)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Tokens"].UpgradeValueBy(1);
        DynamicVars[nameof(NonStackableGutsPower)].UpgradeValueBy(10);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card is CrownedWithLife)
        {
            // 抽到时获得 Guts 效果和能量令牌，然后消耗
            await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature,
                DynamicVars[nameof(NonStackableGutsPower)].BaseValue, Owner.Creature, this);
            await PlayerCmd.GainEnergy(DynamicVars["Tokens"].IntValue, Owner);
            await CardCmd.Exhaust(choiceContext, card, fromHandDraw);
        }
    }
}