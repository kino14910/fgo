using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Cards;

public class QueensCovenant() : FgoCardModel(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<StrengthPower>(2),
        new PowerVar<DexterityPower>(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars[nameof(StrengthPower)].UpgradeValueBy(1);
        DynamicVars[nameof(DexterityPower)].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var strAmount = DynamicVars[nameof(StrengthPower)].BaseValue;

        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature,
            DynamicVars[nameof(DexterityPower)].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<MyFairSoldierPower>(choiceContext, Owner.Creature, strAmount, Owner.Creature, this);
    }
}