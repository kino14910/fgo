using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class ImaginaryOceanBattle : FgoCardModel
{
    public ImaginaryOceanBattle() : base(1, CardType.Skill,
        CardRarity.Rare, TargetType.Self)
    {
        WithNp(5);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var npAmount = DynamicVars["NP"].IntValue + Owner.RunState.TotalFloor;
        await FgoNpCmd.AddNp(npAmount);
        await PowerCmd.Apply<ImaginarySpacePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        await PowerCmd.Apply<WatersidePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        WithCostUpgradeBy(-1);
    }
}