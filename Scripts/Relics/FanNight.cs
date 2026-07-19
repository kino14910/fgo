using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(SharedRelicPool))]
public class FanNight : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override async Task AfterBlockBroken(PlayerChoiceContext choiceContext, Creature target, Creature? breaker)
    {
        if (target == Owner.Creature) return;
        Flash();
        await PowerCmd.Apply<WeakPower>(choiceContext, target, 2m, breaker, choiceContext.LastInvolvedModel as CardModel);
    }
}