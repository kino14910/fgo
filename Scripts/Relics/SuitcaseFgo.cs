using Fgo.Scripts.Character;
using Fgo.Scripts.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Fgo.Scripts.Relics;

[RegisterCharacterStarterRelic(typeof(FgoCharacter))]
public class SuitcaseFgo : FgoRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task BeforeCombatStart()
    {
        Flash();

        await FgoNpCmd.AddNp(20);
    }
}