using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class CrownedWithLife() : FgoCardModel(-2, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CursePower>(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal, CardKeyword.Unplayable];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<CursePower>(1),
        ModCardVars.Int("Star", 10)
    ];

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card is CrownedWithLife)
        {
            await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, DynamicVars[nameof(CursePower)].BaseValue,
                Owner.Creature, this);
            await FgoResCmd.ModifyStars(DynamicVars["Star"].BaseValue, Owner);
        }
    }
}