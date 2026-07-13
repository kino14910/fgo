using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Cards;

public class Coaching() : FgoCardModel(0, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Energy", 2),
        new DamageVar(2, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move),
        new HealVar(2)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var self = Owner.Creature;
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
        await CreatureCmd.Damage(choiceContext, self, DynamicVars.Damage.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, self);
        await CreatureCmd.GainMaxHp(self, DynamicVars.Heal.BaseValue);
    }
}