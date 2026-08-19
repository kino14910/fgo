using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AtTheWell() : FgoCardModel(0, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(6)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(GutsPower)].UpgradeValueBy(6);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var self = Owner.Creature;
        var debuffs = self.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
        foreach (var debuff in debuffs) await PowerCmd.Remove(debuff);
        await PowerCmd.Apply<AtTheWellPower>(choiceContext, self, DynamicVars[nameof(GutsPower)].BaseValue,
            Owner.Creature,
            this);
    }
}