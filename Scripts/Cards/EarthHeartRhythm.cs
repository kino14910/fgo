using Fgo.Scripts.Commands;
using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Fgo.Scripts.Cards;

public class EarthHeartRhythm : FgoCardModel
{
    public EarthHeartRhythm() : base(1, CardType.Skill,
        CardRarity.Uncommon, TargetType.Self)
    {
        WithNp(20);
        WithPower<NpDamagePower>(20, 10);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await FgoNpCmd.AddNp(DynamicVars["NP"].IntValue);
        await PowerCmd.Apply<NpDamagePower>(choiceContext, Owner.Creature,
            DynamicVars[typeof(NpDamagePower).Name].BaseValue, Owner.Creature, this);
    }
}