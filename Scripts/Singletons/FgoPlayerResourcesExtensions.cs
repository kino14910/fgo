using MegaCrit.Sts2.Core.Models;

namespace Fgo.Scripts.Singletons;

public static class FgoPlayerResourcesExtensions
{
    public static FgoPlayerResources FgoRes<T>(this T model) where T : AbstractModel
    {
        return ModelDb.Singleton<FgoPlayerResources>();
    }
}