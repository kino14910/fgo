using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Relics;

public class StargazerTeapot : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        Flash();
        await PowerCmd.Apply<GutsPower>(new BlockingPlayerChoiceContext(), Owner.Creature, 2m, Owner.Creature, null);
    }
}