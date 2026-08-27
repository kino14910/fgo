using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace Fgo.Scripts.Relics;

public class Suitcase : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStartLate()
    {
        Flash();
        await FgoResCmd.ModifyNp(20, Owner);
    }
}