using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Fgo.Scripts.Cards;

public class SpringOfFire() : FgoCardModel(2, CardType.Power,
        CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(20)];

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<NonStackableGutsPower>(choiceContext, Owner.Creature, DynamicVars.Heal.BaseValue,
            Owner.Creature, this);
    }
}