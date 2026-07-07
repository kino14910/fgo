using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class SchwarzwaldFalke : FgoCardModel
{
    public SchwarzwaldFalke() : base(3, CardType.Power,
        CardRarity.Rare, TargetType.Self)
    {
        WithPower<RegenPower>(3);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<NpRatePower>(choiceContext, Owner.Creature, DynamicVars["RegenPower"].BaseValue,
            Owner.Creature, this);
        await PowerCmd.Apply<SchwarzwaldFalkePower>(choiceContext, Owner.Creature, DynamicVars["RegenPower"].BaseValue,
            Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RegenPower"].UpgradeValueBy(2m);
    }
}