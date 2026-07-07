using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class FanNight : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterBlockBroken(Creature creature)
    {
        if (creature == Owner.Creature) return;
        Flash();
        await PowerCmd.Apply<WeakPower>(new BlockingPlayerChoiceContext(), creature, 2m, Owner.Creature, null);
    }
}