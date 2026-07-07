using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Powers;

[RegisterPower(Inherit = true)]
public abstract class FgoPowerModel : ModPowerTemplate
{
    // 自定义图标路径。1:1即可。原版游戏大图256x256，小图64x64。

    public override PowerAssetProfile AssetProfile => new(
        "res://Fgo/images/powers/Power.png",
        "res://Fgo/images/powers/big/Power.png"
    );

    /// <summary>
    ///     公开 Flash 方法，供非 Power 类（如 Card）触发闪光效果。
    /// </summary>
    public void TriggerFlash()
    {
        Flash();
    }
}