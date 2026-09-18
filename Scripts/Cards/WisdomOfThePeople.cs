using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class WisdomOfThePeople() : FgoCardModel(3, CardType.Skill,
    CardRarity.Ancient, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipFactory.FromNp()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Heal(20),
        ModCardVars.Int("Np", 30)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, false);

        var list = GetStatuses(Owner).ToList();
        foreach (var item in list) await CardCmd.Exhaust(choiceContext, item);

        if (IsUpgraded) await FgoResCmd.ModifyNp(DynamicVars["Np"].BaseValue, Owner);
    }

    private static IEnumerable<CardModel> GetStatuses(Player owner)
    {
        return owner.PlayerCombatState!.AllCards.Where(c =>
            c.Type == CardType.Status && c.Pile.Type != PileType.Exhaust);
    }
}