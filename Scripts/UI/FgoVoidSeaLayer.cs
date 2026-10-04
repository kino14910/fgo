using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Fgo.Scripts.UI;

/// <summary>
///     〔虚数空间〕背景的图层脚本。
/// </summary>
/// <remarks>
///     <para>
///         必须是 <see cref="NCombatBackgroundLayer" /> 的子类: 基类 <c>_Ready()</c> 会
///         <c>GetNode&lt;Control&gt;("Visual")</c> 与 <c>GetNode&lt;Control&gt;("PhobiaModeVisual")</c>，
///         两者都不做 null 容忍，缺一个图层进树就抛异常，并且它还要靠这两个节点响应
///         「恐惧模式」开关（<c>UpdatePhobiaMode</c> 在两者间切换显隐）。
///         场景里挂上本类、并保证这两个直属子节点存在，即可满足契约。
///     </para>
///     <para>
///         <b>转场遮罩由 Tween 驱动，而不是 AnimationPlayer。</b>
///         实测（Godot 4.5.1，真实 OpenGL 渲染器）<c>AnimationPlayer</c> 的 value 轨道
///         写 <c>NodePath("X:material/shader_parameter/threshold")</c> 时
///         <b>会被静默忽略</b>：同一条轨道上的 <c>visible</c> 正常生效，唯独 shader 参数读数不动。
///         <c>resource_local_to_scene</c> 不是原因（true/false 两种都失败）。
///         改成 <c>CreateTween()</c> 后同一路径正常，与原版 <c>NTransition.FadeOut</c>
///         用的正是 <c>TweenProperty(material, "shader_parameter/threshold", ...)</c>。
///     </para>
/// </remarks>
public sealed partial class FgoVoidSeaLayer : NCombatBackgroundLayer
{
    /// <summary>进入转场遮罩节点名。缺节点时安静跳过，不影响背景本体。</summary>
    private const string MaskNodePath = "TransitionMask";

    /// <summary>补间属性路径（<c>ShaderMaterial.shader_parameter/&lt;name&gt;</c>）。</summary>
    private static readonly NodePath ThresholdParamPath = new("shader_parameter/threshold");

    private static readonly StringName ThresholdParamName = new("threshold");

    /// <summary>起手保持完全覆盖的时长（秒），让遮罩先「坐实」再开始溶解。</summary>
    private const float CoverSeconds = 0.08f;

    /// <summary>溶解时长（秒）。</summary>
    private const float DissolveSeconds = 0.8f;

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
    ///     本图层每次进入虚数空间都会被 <c>FgoVoidSeaBackground.AddLayerInto</c> 重新实例化，
    ///     所以 <c>_Ready</c> 里起一次补间即可，不需要外部驱动、也不需要复位。
    ///     缓动取 <c>EASE_OUT + TRANS_CUBIC</c>：threshold 走 <c>(1-t)^3</c>，
    ///     也就是「先快速退场、末尾轻柔收束到 0」，末尾不会出现突然消失的硬跳。
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
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenCallback(Callable.From(() => mask.Visible = false));
    }
}
