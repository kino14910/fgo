using Fgo.Scripts.Commands;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.HandSize;

namespace Fgo.Scripts.Cards;

public class SwordOfSelection() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Cards(3),
        ModCardVars.Int("NpPerHands", 2),
        ModCardVars.Computed("Np", static ctx =>
        {
            if (!ctx.HasPlayer) return 0;
            var npPerHands = ctx.GetCardBaseValueOrDefault("NpPerHands");
            var handCount = ctx.Player.PlayerCombatState?.Hand.Cards.Count ?? 0;
            var maxHand = MaxHandSizeCalculator.Calculate(ctx.Player);
            var cards = ctx.GetCardBaseValueOrDefault("Cards");
            return npPerHands * Math.Min(handCount - 1 + cards, maxHand);
        })
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        await FgoResCmd.ModifyNp(this);
    }
}