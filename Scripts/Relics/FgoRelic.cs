using Fgo.Scripts.Character;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Relics;

[RegisterRelic(typeof(FgoRelicPool), Inherit = true)]
// [RegisterCharacterStarterRelic(typeof(FgoCharacter))] // 注册起始遗物
public abstract class FgoRelic : ModRelicTemplate
{
    public override RelicAssetProfile AssetProfile => new(
        // 小图标（原版85x85）
        $"res://Fgo/images/relics/{GetType().Name}.png",
        // 轮廓图标（原版85x85）
        $"res://Fgo/images/relics/{GetType().Name}.png",
        // 大图标（原版256x256）
        $"res://Fgo/images/relics/{GetType().Name}.png"
    );
}