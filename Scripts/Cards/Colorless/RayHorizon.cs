using Fgo.Scripts.Cards.NoblePhantasm;
using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless;

[RegisterCard(typeof(TokenCardPool))]
public class RayHorizon() : FgoBaseCardModel(0, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<InvincibilityTurnPower>(),
        FgoHoverTipHelper.CreateNpHoverTip()
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Np", 50)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Np"].UpgradeValueBy(50);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var npCardPower = await PowerCmd.Apply<NpCardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (npCardPower != null)
            npCardPower.NobleCard = ModelDb.Card<HollowHeartAlbion>();
        await FgoResCmd.ModifyNp(this);
    }
}