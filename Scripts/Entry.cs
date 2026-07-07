using System.Reflection;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
// using Fgo.Scripts.Utils;

namespace Fgo.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Fgo";
    public const string ResPath = $"res://{ModId}";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterModSettingsReflectionProvider<FgoReflectedSettings>();
        FgoEnums.Initialize(ModId);
    }
}