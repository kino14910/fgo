using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Fgo.Scripts.Commands;

public class ReviveCmd
{
    public static async Task Execute(Creature creature, int amount)
    {
        if (creature.CurrentHp < 0) creature.SetCurrentHpInternal(0);
        await CreatureCmd.Heal(creature, amount);
    }
}