using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Fgo.Scripts.Relics;

public class BB : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task BeforeCombatStart()
    {
        Flash();
        var self = Owner.Creature;
        var roll = Random.Shared.Next(2);

        if (roll == 0)
        {
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), self, 2m, self, null);
            await PowerCmd.Apply<DexterityPower>(new BlockingPlayerChoiceContext(), self, 2m, self, null);
        }
        else
        {
            var debuffRoll = Random.Shared.Next(3);
            if (debuffRoll == 0)
                await PowerCmd.Apply<VulnerablePower>(new BlockingPlayerChoiceContext(), self, 1m, self, null);
            else if (debuffRoll == 1)
                await PowerCmd.Apply<WeakPower>(new BlockingPlayerChoiceContext(), self, 1m, self, null);
            else
                await PowerCmd.Apply<FrailPower>(new BlockingPlayerChoiceContext(), self, 1m, self, null);
        }
    }
}