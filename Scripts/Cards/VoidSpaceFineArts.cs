using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class VoidSpaceFineArts() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<GutsPower>(),
        HoverTipFactory.FromPower<CursePower>()
    ];

    protected override HashSet<CardTag> CanonicalTags => [FgoTags.Foreigner];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<GutsPower>(10),
        ModCardVars.Int("CurseStacks", 5),
        ModCardVars.Computed("Np", static ctx =>
                ctx.BaseValue * ctx.Player?.Creature.GetPowerAmount<CursePower>() ?? 0,
            5)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["CurseStacks"].UpgradeValueBy(5);
        DynamicVars["Np"].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<GutsPower>(choiceContext, Owner.Creature, DynamicVars[nameof(GutsPower)].BaseValue,
            Owner.Creature,
            this);
        for (var i = 0; i < 3; i++)
            await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

        await FgoResCmd.ModifyNp(DynamicVars.EvaluateValueOrDefault("Np"), cardPlay.Player);
    }
}