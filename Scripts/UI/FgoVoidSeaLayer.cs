using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <summary>
///     〔虚数空间〕背景的图层脚本。
/// </summary>
/// <remarks>
///     <para>
///         必须是 <see cref="NCombatBackgroundLayer" /> 的子类：基类 <c>_Ready()</c> 非 null 容忍地取
///         <c>Visual</c> 与 <c>PhobiaModeVisual</c> 并据「恐惧模式」切换显隐，场景须保证两直属子节点存在。
///     </para>
///     <para>
///         <b>转场遮罩用 Tween 而非 AnimationPlayer</b>：实测 Godot 4.5.1 OpenGL 下 AnimationPlayer 写
///         <c>shader_parameter/threshold</c> 会被静默忽略（同轨 visible 正常），原因非 <c>resource_local_to_scene</c>；
///         改 CreateTween 后同一路径正常，与原版 <c>NTransition.FadeOut</c> 一致。
///     </para>
/// </remarks>
public sealed partial class FgoVoidSeaLayer : NCombatBackgroundLayer
{
    private const string MaskNodePath = "TransitionMask";

    /// <summary>
    ///     起手保持完全覆盖的时长（秒）。
    ///     <para>
    ///         要盖住虚数之海本体的入场淡入（<c>FgoVoidSeaBackground.EnterFadeSeconds = 0.25</c>）
    ///         再留一点「深渊先成形」的读图时间，所以必须 &gt; 入场淡入。
    ///     </para>
    /// </summary>
    private const float CoverSeconds = 0.40f;

    /// <summary>
    ///     溶解时长（秒）。原版角色选人转场 0.8s 是全黑幕退场、无需辨认；本遮罩是「有内容的画面自左向右溶解」，沿用 0.8s 实测过快看不清，故取 1.5s（≈原版 1.9 倍），整段入场约 0.40+1.50=1.9s。
    /// </summary>
    private const float DissolveSeconds = 1.5f;

    /// <summary>补间属性路径（<c>ShaderMaterial.shader_parameter/&lt;name&gt;</c>）。</summary>
    private static readonly NodePath ThresholdParamPath = new("shader_parameter/threshold");

    private static readonly StringName ThresholdParamName = new("threshold");

    public override void _Ready()
    {
        // 基类负责 Visual / PhobiaModeVisual 的恐惧模式切换，必须先跑。
        base._Ready();

        PlayEnterTransition();
    }

    /// <summary>
    ///     播放「进入〔虚数之海〕」溶解：遮罩从完全覆盖（threshold=1）溶到全透明（threshold=0），
    ///     收尾把节点本身 <c>Visible=false</c>，避免战斗全程白跑一个全屏 shader。
    /// </summary>
    /// <remarks>
    ///     本图层每次进入虚数空间都由 <c>FgoVoidSeaBackground.AddLayerInto</c> 重新实例化，故 <c>_Ready</c> 起一次补间即可、无需外部驱动或复位。
    ///     缓动取 <c>EASE_IN_OUT + TRANS_CUBIC</c>（S 形）而非 <c>EASE_OUT</c>：threshold 单调递减且首尾速度趋 0，起手不褪大块、收尾不突然消失，对应「无闪烁、无跳变」；shader 的 cover 不含 TIME，中途无抖动。
    /// </remarks>
    private void PlayEnterTransition()
    {
        var mask = GetNodeOrNull<ColorRect>(MaskNodePath);
        if (mask?.Material is not ShaderMaterial material) return;

        mask.Visible = true;
        material.SetShaderParameter(ThresholdParamName, 1f);

        var tween = CreateTween();
        tween.TweenInterval(CoverSeconds);
        tween.TweenProperty(material, ThresholdParamPath, 0f, DissolveSeconds)
            .SetEase(Tween.EaseType.InOut)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => mask.Visible = false));
    }
}