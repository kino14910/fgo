using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class WisdomOfThePeople() : FgoCardModel(3, CardType.Skill,
    CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Heal(20),
        ModCardVars.Int("Np", 30)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);

        var debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
        if (debuffs.Count > 0)
        {
            var random = debuffs[Owner.RunState.Rng.Niche.NextInt(debuffs.Count)];
            await PowerCmd.Remove(random);
        }

        if (IsUpgraded) await FgoResCmd.ModifyNp(this);
    }
}