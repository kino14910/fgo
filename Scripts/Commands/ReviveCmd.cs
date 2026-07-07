using Fgo.Scripts.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Fgo.Scripts.Commands;

public class ReviveCmd
{
    public static async Task Execute(Creature creature, int amount)
    {
        await CreatureCmd.Heal(creature, amount);
        await PowerCmd.Remove<GutsPower>(creature);
    }
}