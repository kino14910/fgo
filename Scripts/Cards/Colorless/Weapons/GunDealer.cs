using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Cards.Colorless.Weapons;

[RegisterCard(typeof(TokenCardPool))]
public class GunDealer() : FgoBaseCardModel(0, CardType.Skill,
    CardRarity.Token, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard<Pistol>(),
        HoverTipFactory.FromCard<SMG>(),
        HoverTipFactory.FromCard<SniperRifle>(),
        HoverTipFactory.FromCard<RocketLauncher>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var options = new List<CardModel>
        {
            combat.CreateCard<Pistol>(Owner),
            combat.CreateCard<SMG>(Owner),
            combat.CreateCard<SniperRifle>(Owner),
            combat.CreateCard<RocketLauncher>(Owner)
        };

        if (IsUpgraded)
            CardCmd.Upgrade(options, CardPreviewStyle.None);

        var selected = await FgoCardActions.FromChooseACardScreenUncapped(choiceContext, options, Owner);

        if (selected == null) return;

        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, Owner);
    }
}