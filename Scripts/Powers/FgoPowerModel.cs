using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower(Inherit = true)]
public abstract class FgoPowerModel : ModPowerTemplate
{
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。

    // public override PowerAssetProfile AssetProfile => new(
    //     "res://Fgo/images/powers/Power.png",
    //     "res://Fgo/images/powers/big/Power.png"
    // );

    private static readonly Dictionary<string, PowerAssetProfile> ProfileCache = new();

    public override PowerAssetProfile AssetProfile
    {
        get
        {
            var typeName = GetType().Name;
            var iconPath = $"res://Fgo/images/powers/{typeName}.png";
            var bigIconPath = $"res://Fgo/images/powers/{typeName}.png";

            if (ProfileCache.TryGetValue(typeName, out var cached))
                return cached;

            var iconExists = ResourceLoader.Exists(iconPath);
            var profile = iconExists
                ? new PowerAssetProfile(iconPath, bigIconPath)
                : new PowerAssetProfile("res://Fgo/images/powers/Power.png", "res://Fgo/images/powers/big/Power.png");

            ProfileCache[typeName] = profile;
            return profile;
        }
    }
}