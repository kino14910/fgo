using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace Fgo.Scripts.Character;

public class FgoCardPool : TypeListCardPoolModel
{
    // 根据你使用的卡框决定使用哪个Material
    // private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateReplaceHueShaderMaterial(0.5f, 0.5f, 1f); // 如果你使用原版卡框，使用这个直接替换色调。
    // private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateRgbShaderMaterial(0.5f, 0.5f, 1f); // 使用原版卡框替换色调。除非你的版本没有CreateReplaceHueShaderMaterial函数，否则应使用上面那种
    private static readonly Material?
        _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial(); // 如果你是自定义卡框，使用这个

    // 卡池的ID。必须唯一防撞车。
    public override string Title => "fgo";
    public override string EnergyColorName => "fgo";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Fgo/images/ui/energy_fgo.png";

    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Fgo/images/ui/energy_fgo_big.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.42f, 0.36f, 0.08f);

    // 能量表盘文字轮廓颜色
    public override Color EnergyOutlineColor => new(0.42f, 0.36f, 0.08f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}