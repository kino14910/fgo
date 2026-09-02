using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards;

[RegisterCharacterStarterCard(typeof(FgoCharacter))]
public class DreamUponTheStars() : FgoCardModel(1, CardType.Skill,
    CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<NpDamagePower>(),
        HoverTipFactory.FromPower<OverchargePower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Power<NpDamagePower>(20),
        ModCardVars.Power<OverchargePower>(1),
        ModCardVars.Int("Np", 10)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(NpDamagePower)].UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(NpDamagePower)].BaseValue, Owner.Creature, this);
        await FgoResCmd.ModifyNp(this);
        await PowerCmd.Apply<OverchargePower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(OverchargePower)].BaseValue, Owner.Creature, this);
    }
}