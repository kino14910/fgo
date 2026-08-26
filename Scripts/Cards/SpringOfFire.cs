using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class SpringOfFire() : FgoCardModel(3, CardType.Power,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<SpringOfFireGutsPower>(),
        HoverTipFactory.FromPower<SpringOfFirePower>(),
        HoverTipFactory.FromPower<NpDamagePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [ModCardVars.Heal(3)];

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(3);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var guts = await PowerCmd.Apply<SpringOfFireGutsPower>(choiceContext, Owner.Creature,
            DynamicVars.Heal.BaseValue,
            Owner.Creature, this);
        if (guts != null) guts.Times = 3;
        await PowerCmd.Apply<SpringOfFirePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(SpringOfFirePower)].BaseValue,
            Owner.Creature, this);
    }
}