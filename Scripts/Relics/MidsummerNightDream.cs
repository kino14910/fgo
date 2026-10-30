using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Relics;

public class MidsummerNightDream : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not CursePower || amount <= 0m || power.Owner != Owner.Creature)
            return Task.CompletedTask;

        Flash();
        return PowerCmd.Apply<RegenPower>(choiceContext, Owner.Creature, amount,
            Owner.Creature, null);
    }
}