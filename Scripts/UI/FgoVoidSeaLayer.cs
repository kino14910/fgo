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

    /// <summary>
    ///     起手保持完全覆盖的时长（秒）。
    ///     <para>
    ///         要盖住虚数之海本体的入场淡入（<c>FgoVoidSeaBackground.EnterFadeSeconds = 0.25</c>）
    ///         再留一点「深渊先成形」的读图时间，所以必须 &gt; 入场淡入。
    ///     </para>
    /// </summary>
    private const float CoverSeconds = 0.40f;

    /// <summary>
    ///     溶解时长（秒）。
    ///     <para>
    ///         原版角色选人转场是 <c>NTransition.FadeOut</c> 的 <b>0.8s</b>，但那是全黑幕退场、
    ///         没有内容要辨认；本遮罩是「有内容的画面自左向右溶解」，沿用 0.8s 时
    ///         肉眼几乎只看到一闪而过（实测反馈：过快、看不清）。
    ///         取 <b>1.5s</b> = 原版的 1.9 倍，配合下面的保持段，整段入场约
    ///         <c>0.40 + 1.50 = 1.9s</c>，足够看清溶解锋面推过全屏。
    ///     </para>
    /// </summary>
    private const float DissolveSeconds = 1.5f;

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
    ///     <para>
    ///         本图层每次进入虚数空间都会被 <c>FgoVoidSeaBackground.AddLayerInto</c> 重新实例化，
    ///         所以 <c>_Ready</c> 里起一次补间即可，不需要外部驱动、也不需要复位。
    ///     </para>
    ///     <para>
    ///         <b>缓动取 <c>EASE_IN_OUT + TRANS_CUBIC</c>（S 形）而不是 <c>EASE_OUT</c>。</b>
    ///         threshold 单调下降，且首尾速度都趋 0：起手不会「一上来就褪掉一大块」，
    ///         收尾也不会在最后一帧突然消失 —— 正好对应「无闪烁、无突然跳变」。
    ///         运动过程完全由 threshold 单调驱动，shader 的 cover 不含 TIME，故中途无抖动。
    ///     </para>
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
