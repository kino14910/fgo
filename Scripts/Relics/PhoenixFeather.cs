using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

/// <summary>
///     凤凰羽毛: 商店遗物。<see cref="Powers.GutsPower" /> /
///     <see cref="Powers.NonStackableGutsPower" /> 发动（免死）后，额外恢复最大生命值的 10%。
///     由毅力能力的 AfterPreventingDeath 末尾按玩家查表调用，仅对遗物持有者本人生效。
/// </summary>
public class PhoenixFeather : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public async Task OnGutsTriggered(Creature creature)
    {
        if (Owner is not { } owner || owner.Creature != creature) return;
        Flash();
        await CreatureCmd.Heal(creature, creature.MaxHp * 0.1m);
    }
}
