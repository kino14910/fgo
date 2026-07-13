using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Fgo.Scripts.Cards;

public class MaidenOfAFlowerPatio() : FgoCardModel(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(15, ValueProp.Move),
        new PowerVar<CursePower>(1)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(5);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<CursePower>(choiceContext, Owner.Creature, DynamicVars[nameof(CursePower)].BaseValue,
            Owner.Creature, this);
    }
}