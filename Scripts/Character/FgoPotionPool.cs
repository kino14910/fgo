using STS2RitsuLib.Scaffolding.Content;

namespace Fgo.Scripts.Character;

public class FgoPotionPool : TypeListPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Fgo/images/ui/energy_fgo.png";

    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Fgo/images/ui/energy_fgo_big.png";

    public override string EnergyColorName => "fgo";
}