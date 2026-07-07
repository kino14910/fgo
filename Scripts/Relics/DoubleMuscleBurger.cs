using Fgo.Scripts.Cards.Colorless;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

public class DoubleMuscleBurger : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterObtained()
    {
        await CreatureCmd.GainMaxHp(Owner.Creature, 40);
        await CardPileCmd.AddCursesToDeck(Enumerable.Range(0, 3)
            .Select(_ => ModelDb.Card<Dumuzid>().ToMutable()), Owner);
    }
}