using Fgo.Scripts.Cards.NoblePhantasm;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

public class BowInsignia : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterObtained()
    {
        await CardPileCmd.Add(ModelDb.Card<Unlimited>().ToMutable(), Owner.Deck, CardPilePosition.Top, this);
    }
}