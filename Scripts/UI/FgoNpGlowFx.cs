using Godot;
using STS2RitsuLib;

namespace Fgo.Scripts.UI;

/// <summary>
///     NP 按钮的发光粒子层的场景宿主：只负责持有节点引用、定位光心、转发强度。
/// </summary>
/// <remarks>
///     <para>
///         <b>节点结构</b>（见 <c>res://Fgo/scenes/fgo_np_glow_fx.tscn</c>）：
///     </para>
///     <list type="number">
///         <item>
///             <b>Rays</b> —— <see cref="ColorRect" /> + <c>res://Fgo/shaders/np_gold_rays.gdshader</c>。
///             光核 / 辉光 / 拉丝 / 径向光芒 / 星星粒子全部在片元里解析生成，无贴图。
///         </item>
///         <item>
///             <b>Sparks</b> —— <see cref="GpuParticles2D" />，贴图 <c>res://Fgo/images/ui/np_spark.png</c>。
///             纯粹为光效补充空气浮尘质感，参数同样由 <see cref="NpGlowTuning" /> 驱动。
///         </item>
///     </list>
///     <para>
///         <b>参数不在本类</b>：全部走 <see cref="FgoNpBar.NpGlowTuning" /> +
///         <see cref="FgoNpBar.ApplyGlowTuning" />，本类不保存任何视觉默认值，
///         以免出现「场景一套、Export 一套、代码又一套」的三重真相。
///     </para>
///     <para>
///         <b>层级</b>：本层要绘制在 <c>NpButton</c> <i>之下</i>（按钮保持在最前，
///         避免加色叠加把暖橙色的按钮贴图与 NP 数字泛白）。由调用方
///         （<see cref="FgoNpBar.BuildGlowFx" />）设置 <see cref="CanvasItem.ZIndex" />
///         与树序，本类不参与。
///     </para>
///     <para>
///         <b>鼠标</b>：本层必须完全不参与命中测试，否则会吃掉点击。
///         根节点与 <c>Rays</c> 在场景里已设 <c>mouse_filter = Ignore</c>，此处再兜底设一次；
///         <c>Sparks</c> 是 Node2D，本身不参与 GUI 命中测试。
///     </para>
/// </remarks>
public sealed partial class FgoNpGlowFx : Control
{
    /// <summary>光效层场景路径。节点结构与美术默认值都在里面。</summary>
    public const string ScenePath = "res://Fgo/scenes/fgo_np_glow_fx.tscn";

    /// <summary>
    ///     特效层尺寸（像素）。与场景里根节点的 <c>offset_right/bottom</c> 对应。
    /// </summary>
    public static readonly Vector2 FxSize = new(260f, 200f);

    /// <summary>
    ///     光心在本层内的归一化位置。固定为正中，<see cref="FgoNpBar" /> 每帧把本层
    ///     对齐到按钮中心，因此星星从按钮中心向四周发射。
    /// </summary>
    public static readonly Vector2 OriginUv = new(0.5f, 0.5f);

    /// <summary>着色器里控制总亮度的 uniform 名。</summary>
    private static readonly StringName IntensityParam = "intensity";

    /// <summary>场景缓存。mod 自带场景不在房间预加载集里，自己缓存避免重复查盘与告警。</summary>
    private static PackedScene? _scene;

    private ShaderMaterial? _rayMaterial;
    private ColorRect? _rays;
    private GpuParticles2D? _sparks;

    /// <summary>解析光效的着色器材质，供 <see cref="FgoNpBar.ApplyGlowTuning" /> 写 uniform。</summary>
    public ShaderMaterial? RayMaterial => _rayMaterial;

    /// <summary>
    ///     实例化光效层场景。调用方负责 <c>AddChild</c> 与层级设置。
    /// </summary>
    public static FgoNpGlowFx? Create()
    {
        _scene ??= ResourceLoader.Load<PackedScene>(ScenePath);

        if (_scene == null)
        {
            Entry.Logger.ErrorNoTrace($"[Fgo] NpGlowFx: scene not found: {ScenePath}");
            return null;
        }

        var fx = _scene.Instantiate<FgoNpGlowFx>();

        if (fx == null)
            Entry.Logger.ErrorNoTrace(
                $"[Fgo] NpGlowFx: scene root is not {nameof(FgoNpGlowFx)} " +
                "(check the root node's script in the .tscn).");

        return fx;
    }

    public override void _Ready()
    {
        // 兜底：整层不参与命中测试，否则会挡住下方按钮的点击。
        MouseFilter = MouseFilterEnum.Ignore;
        Size = FxSize;

        _rays = GetNodeOrNull<ColorRect>("%Rays");
        _sparks = GetNodeOrNull<GpuParticles2D>("%Sparks");
        _rayMaterial = _rays?.Material as ShaderMaterial;

        if (_rays != null)
            _rays.MouseFilter = MouseFilterEnum.Ignore;

        if (_rayMaterial == null)
            Entry.Logger.ErrorNoTrace("[Fgo] NpGlowFx: %Rays node or its ShaderMaterial is missing.");

        if (_sparks == null)
            Entry.Logger.ErrorNoTrace("[Fgo] NpGlowFx: %Sparks node missing.");

        // 光心与 aspect 属于几何量，场景加载即可确定，不随调参变化。
        _rayMaterial?.SetShaderParameter("origin", OriginUv);
        _rayMaterial?.SetShaderParameter("aspect", FxSize.X / FxSize.Y);
    }

    /// <summary>把光心对准父节点局部坐标里的某一点（调用方传按钮中心）。</summary>
    public void SetOriginLocal(Vector2 originLocal)
    {
        Position = originLocal - OriginUv * FxSize;
    }

    /// <summary>
    ///     光尘粒子层参数。解析光效的 uniform 由 <see cref="FgoNpBar.ApplyGlowTuning" /> 写，
    ///     这里只处理 <see cref="GpuParticles2D" /> 侧的属性。
    /// </summary>
    public void ApplyEmberTuning(NpGlowTuning t)
    {
        if (_sparks == null)
            return;

        _sparks.Amount = Mathf.Max(t.EmberAmount, 1);
        _sparks.Lifetime = t.EmberLifetime;

        if (_sparks.ProcessMaterial is ParticleProcessMaterial pm)
        {
            pm.InitialVelocityMin = t.EmberSpeedMin;
            pm.InitialVelocityMax = t.EmberSpeedMax;
            pm.RadialAccelMin = t.EmberAccelMax * 0.35f;
            pm.RadialAccelMax = t.EmberAccelMax;
            pm.ScaleMin = t.EmberScaleMin;
            pm.ScaleMax = t.EmberScaleMax;
        }

        // 粒子活动包络必须跟着参数走，否则改大射程后粒子会飞出矩形被 Godot 静默丢弃
        // （越界粒子不报错、日志无痕，是"粒子完全不出现"的头号原因）。
        _sparks.VisibilityRect = ComputeEmberVisibilityRect(t);
    }

    /// <summary>
    ///     0 = 完全熄灭，1 = 满亮度（&gt;1 会让高光过曝，用于点击爆发）。
    /// </summary>
    /// <remarks>
    ///     解析光效走着色器 <c>intensity</c> uniform；光尘粒子走
    ///     <see cref="GpuParticles2D.AmountRatio" />，这样密度与亮度同步淡入淡出。
    /// </remarks>
    public void SetIntensity(float value)
    {
        var v = Mathf.Max(value, 0f);

        _rayMaterial?.SetShaderParameter(IntensityParam, v);

        if (_sparks == null)
            return;

        // AmountRatio 上限为 1，强度再高也不会让粒子数变多，只会更亮。
        _sparks.AmountRatio = Mathf.Clamp(v, 0f, 1f);

        var emitting = v > 0.01f;

        if (_sparks.Emitting != emitting)
            _sparks.Emitting = emitting;
    }

    /// <summary>
    ///     按「初速 + 径向加速度 + 寿命」估算光尘的最大飞行距离，据此留出可见矩形。
    /// </summary>
    /// <remarks>
    ///     矩形是相对粒子节点自身原点的；粒子已位于光心（<see cref="OriginUv" /> × <see cref="FxSize" />），
    ///     所以这里直接以光心为圆心向外扩一个半径即可。
    /// </remarks>
    private static Rect2 ComputeEmberVisibilityRect(NpGlowTuning t)
    {
        var lifetime = Mathf.Max(t.EmberLifetime, 0.01f);

        // 径向匀加速上界：s = v·t + ½·a·t²。取最大初速与最大加速度（忽略阻尼，偏保守）。
        // 这里必须用 t.EmberAccelMax —— 早前把它硬编码成 40，与场景里的实际值脱节，
        // 改小半径后矩形会算得过大（多绘制的空白区，浪费但不出错），
        // 反向脱节则会「粒子飞出矩形被静默丢弃」，那才是真的看不见。
        var reach = t.EmberSpeedMax * lifetime + 0.5f * t.EmberAccelMax * lifetime * lifetime;

        // 贴图是 64px，EmberScaleMax 是缩放系数，再留一点 turbulence 抖动余量。
        var pad = 64f * t.EmberScaleMax + 24f;
        var half = reach + pad;

        return new Rect2(-half, -half, half * 2f, half * 2f);
    }

    /// <summary>
    ///     运行期诊断：把光效层与粒子层的实际矩形、可见性、裁剪链打出来。
    ///     粒子的失效路径（贴图缺失、<c>visibility_rect</c> 错位、被遮挡）在 Godot 侧全是静默的，
    ///     光看画面无法区分，只能靠这几个数值定位。
    /// </summary>
    public void DumpRuntimeState(string tag)
    {
        Entry.Logger.Info(
            $"[Fgo] NpGlowFx[{tag}]: size={Size} pos={Position} globalPos={GlobalPosition} " +
            $"rect={GetGlobalRect()} visible={Visible} zIndex={ZIndex} zRel={ZAsRelative} " +
            $"clip={ClipContents} mouse={MouseFilter} parent={GetParent()?.GetType().Name ?? "<null>"}");

        if (_rays != null)
            Entry.Logger.Info(
                $"[Fgo] NpGlowFx[{tag}]: rays size={_rays.Size} visible={_rays.Visible} " +
                $"shader={_rayMaterial?.Shader != null} " +
                $"intensity={_rayMaterial?.GetShaderParameter("intensity")}");

        if (_sparks == null)
        {
            Entry.Logger.ErrorNoTrace($"[Fgo] NpGlowFx[{tag}]: sparks node missing.");
            return;
        }

        Entry.Logger.Info(
            $"[Fgo] NpGlowFx[{tag}]: sparks globalPos={_sparks.GlobalPosition} visible={_sparks.Visible} " +
            $"emitting={_sparks.Emitting} ratio={_sparks.AmountRatio} texture={_sparks.Texture != null} " +
            $"amount={_sparks.Amount} lifetime={_sparks.Lifetime} preprocess={_sparks.Preprocess} " +
            $"localCoords={_sparks.LocalCoords} visibilityRect={_sparks.VisibilityRect}");
    }
}
