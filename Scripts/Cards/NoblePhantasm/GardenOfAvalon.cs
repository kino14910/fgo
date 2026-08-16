using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards.NoblePhantasm;

public class GardenOfAvalon() : NobleCardModel(1, CardType.Power, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpPerTurnPower>(),
        HoverTipFactory.FromPower<StarsPerTurnPower>(),
        FgoHoverTipHelper.CreateNpHoverTip(),
        FgoHoverTipHelper.CreateStarHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpPerTurnPower>(5),
        ModCardVars.Power<StarsPerTurnPower>(5),
        ModCardVars.Power<RegenPower>(3)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(RegenPower)].UpgradeValueBy(1);
        DynamicVars[nameof(StarsPerTurnPower)].UpgradeValueBy(5);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(RegenPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<NpPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpPerTurnPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StarsPerTurnPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(StarsPerTurnPower)].BaseValue, Owner.Creature, this);
    }
}