using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class WaterfrontSaintess : FgoCardModel
{
    public WaterfrontSaintess() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithNp(20, 10);
        WithPower<NpDamagePower>(20, 10);
        WithVar("CritDamage", 30);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
        if (Owner.Creature.HasPower<WatersidePower>())
            await PowerCmd.Apply<CriticalDamagePower>(choiceContext, Owner.Creature,
                DynamicVars["CritDamage"].BaseValue, Owner.Creature, this);
    }
}