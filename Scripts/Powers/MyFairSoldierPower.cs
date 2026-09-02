using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

public class MyFairSoldierPower : FgoPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/AfterDurationDebuffPower.png",
        "res://Fgo/images/powers/big/AfterDurationDebuffPower.png"
    );

    private CardModel? _grantingCard;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _grantingCard = cardSource;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card == _grantingCard) return;

        if (cardPlay.Card.Type == CardType.Power)
        {
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), Owner, -Amount, Owner, null);
            await PowerCmd.Apply<DexterityPower>(new BlockingPlayerChoiceContext(), Owner, -Amount, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}