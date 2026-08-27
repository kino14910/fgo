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

public class LakeTexcoco() : FgoCardModel(1, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<LakeTexcocoPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 20),
        ModCardVars.Power<LakeTexcocoPower>(10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FgoResCmd.ModifyNp(this);
        await PowerCmd.Apply<WatersidePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<LakeTexcocoPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(LakeTexcocoPower)].BaseValue, Owner.Creature, this);
    }
}