using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class BlessingOfKur() : FgoCardModel(1, CardType.Power,
    CardRarity.Uncommon, TargetType.Self)
{
    private const int BaseStacks = 2;
    private const int UpgradeStacks = 1;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<KurKigalIrkalla>(),
        HoverTipFactory.FromPower<BlessingOfKurPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<BlessingOfKurPower>(BaseStacks),
        ModCardVars.Power<MaxHpPower>(BaseStacks * BlessingOfKurPower.MaxHpPerStack)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(BlessingOfKurPower)].UpgradeValueBy(UpgradeStacks);
        DynamicVars[nameof(MaxHpPower)].UpgradeValueBy(UpgradeStacks * BlessingOfKurPower.MaxHpPerStack);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        await PowerCmd.Apply<BlessingOfKurPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(BlessingOfKurPower)].BaseValue,
            Owner.Creature, this);
    }
}