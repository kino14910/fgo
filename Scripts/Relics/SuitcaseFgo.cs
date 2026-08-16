using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

public class SuitcaseFgo : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        Flash();
        await FgoResCmd.ModifyNp(100, Owner);
    }
}