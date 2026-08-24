using Fgo.Scripts.Commands;
using Fgo.Scripts.Singletons;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class AnimalDialogue() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Threshold", 20),
        ModCardVars.ComputedEnergy("Energy", ctx =>
        {
            var threshold = ctx.GetCardIntOrDefault("Threshold", 20);
            if (!ctx.HasPlayer || threshold <= 0) return 0m;
            return FgoBattleHooks.Get(ctx.Player).Np / threshold;
        })
    ];

    protected override bool ShouldGlowGoldInternal =>
        IsMutable && Owner is not null &&
        FgoBattleHooks.Get(Owner).Np >= DynamicVars["Threshold"].IntValue;

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.EvaluateValueOrDefault("Energy"), Owner);
        await FgoResCmd.ResetNp(Owner);
    }
}