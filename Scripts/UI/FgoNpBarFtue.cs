using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace Fgo.Scripts.UI;

/// <summary>
///     「宝具条已满」的 FTUE 教学弹窗，等价于原版 <c>NCombatRewardFtue</c> 的单页结构。
/// </summary>
/// <remarks>
///     <para>
///         场景 <c>res://Fgo/scenes/fgo_np_bar_ftue.tscn</c> 直接照搬原版
///         <c>scenes/ftue/combat_reward_ftue.tscn</c> 的节点结构，
///         只把 <c>res://images|themes|shaders|addons|src</c> 换成了 <c>res://Fgo/**</c> 与
///         <c>res://Scripts/**</c>（mod pck 只打包这两个前缀）。
///     </para>
///     <para>
///         <b>固定节点路径</b>（与原版一致，便于对照）：
///         <list type="bullet">
///             <item><c>FtuePopup/Header</c> —— 原版是 MegaLabel，这里是引擎 Label + <see cref="FgoFtueText" /> 复刻 auto-size</item>
///             <item><c>FtuePopup/DescriptionContainer/Description</c> —— 原版是 MegaRichTextLabel，这里是 RichTextLabel + 手动装 <c>[gold]</c> 等效果</item>
///             <item><c>FtuePopup/FtueConfirmButton</c> —— <see cref="FgoFtueConfirmButton" />（NButton 子类）</item>
///         </list>
///     </para>
///     <para>
///         <b>位置与原版不同</b>：原版把弹窗固定在屏幕右下角（那是战斗奖励的位置），
///         本弹窗必须贴着 NP 按钮弹，所以由 <see cref="Create" /> 传入按钮的屏幕矩形，
///         <see cref="LayoutTowards" /> 在运行时重摆 <c>FtuePopup</c> 与 <c>Arrow</c>。
///     </para>
///     <para>
///         <b>焦点与热键屏蔽</b>：基类 <see cref="NFtue._EnterTree" /> 已
///         <c>AddBlockingScreen(this)</c> 并把四个 <c>FocusNeighbor*</c> 指向自身，无需重复处理。
///         <c>CloseFtue()</c> 是 <c>protected</c>，本类直接调用即可。
///     </para>
/// </remarks>
public sealed partial class FgoNpBarFtue : NFtue
{
    private const string TitleKey = "FGO_NP_FTUE_TITLE";
    private const string DescriptionKey = "FGO_NP_FTUE_DESCRIPTION";

    /// <summary>弹窗与目标之间的水平间距（目标在左、弹窗在右时用）。</summary>
    private const float GapHorizontal = 110f;

    /// <summary>弹窗距离屏幕边缘的最小留白。</summary>
    private const float ScreenMargin = 24f;

    /// <summary>箭头缩放。1 是贴图原始尺寸，对 244×204 的原图来说太大。</summary>
    private const float ArrowScale = 0.5f;

    /// <summary>
    ///     箭头贴图尖端相对 <c>PivotOffset</c> 的偏移。
    /// </summary>
    /// <remarks>
    ///     由贴图像素扫出来的：244×204 的图里红色最左下的像素在 (16, 198)，
    ///     而 <c>pivot_offset</c> 是 (122, 102)（贴图中心），差值即此。
    ///     <b>必须瞄尖端而不是 pivot</b>：尖端离 pivot 有 143px，
    ///     瞄 pivot 会让箭头实际指到目标前方 143px 的地方，只是碰巧看着还行。
    /// </remarks>
    private static readonly Vector2 ArrowTipOffset = new(-106f, 96f);

    /// <summary>
    ///     箭头未旋转时尖端的朝向（y-down 坐标，+X 为 0°、+Y 为 90°）。
    /// </summary>
    /// <remarks>
    ///     由 <see cref="ArrowTipOffset" /> 反推：<c>atan2(96, -106) ≈ 137.83°</c>。
    ///     贴图默认朝向左下，指向性旋转就是「目标方位角 - 这个值」。
    /// </remarks>
    private const float ArrowDefaultBearingDeg = 137.83f;

    private Label? _header;
    private RichTextLabel? _description;
    private NButton? _confirmButton;
    private Control? _popup;
    private TextureRect? _arrow;

    /// <summary>
    ///     NP 按钮在屏幕坐标系里的矩形，由 <see cref="Create" /> 注入。
    /// </summary>
    /// <remarks>
    ///     NP 条挂在 <c>NCreatureStateDisplay</c> 下、跟着 HP 条走
    ///     （见 <c>FgoNpBar.SyncWithHealthBar</c>），运行期位置随分辨率与玩家位置变化，
    ///     场景里写死的 offset 一定对不上，只能运行时算。
    /// </remarks>
    private Rect2 _targetRect;

    /// <summary>
    ///     用预加载场景创建实例。
    /// </summary>
    /// <param name="targetRect">
    ///     NP 按钮在屏幕坐标系里的矩形，用于把弹窗摆到按钮旁边、箭头指向按钮。
    /// </param>
    /// <remarks>
    ///     原版走 <c>PreloadManager.Cache.GetScene</c>，那是主程序集的场景缓存；
    ///     mod 场景不在里面，直接 <c>GD.Load</c>。路径写死以保证是常量、
    ///     Godot 能在导出时把它算进 pck 依赖。
    /// </remarks>
    public static FgoNpBarFtue? Create(Rect2 targetRect)
    {
        if (TestMode.IsOn)
            return null;

        var ftue = GD.Load<PackedScene>(ScenePath)
            .Instantiate<FgoNpBarFtue>(PackedScene.GenEditState.Disabled);

        ftue._targetRect = targetRect;
        return ftue;
    }

    private static string ScenePath => $"{Entry.ResPath}/scenes/fgo_np_bar_ftue.tscn";

    public override void _Ready()
    {
        _popup = GetNode<Control>("FtuePopup");
        _arrow = GetNode<TextureRect>("Arrow");
        _header = GetNode<Label>("FtuePopup/Header");
        _description = GetNode<RichTextLabel>("FtuePopup/DescriptionContainer/Description");
        _confirmButton = GetNode<NButton>("FtuePopup/FtueConfirmButton");

        FgoFtueText.SetAutoSizeText(_header, new LocString("ftues", TitleKey).GetFormattedText(), 14, 26);
        FgoFtueText.SetAutoSizeText(
            _description,
            new LocString("ftues", DescriptionKey).GetFormattedText(),
            12,
            22
        );

        _confirmButton.Connect(
            NClickableControl.SignalName.Released,
            Callable.From((Action<NButton>)OnConfirmReleased)
        );

        // 同步摆位：_popup 的尺寸来自它自己的 offset（anchors_preset=3 + 绝对 offset），
        // 不依赖父节点布局是否解析完，所以 _Ready 里直接算就是准的。
        // 用 deferred 反而会在首帧渲染前多出一帧「原版右下角位置」的闪烁。
        LayoutTowards(_targetRect);
        _confirmButton.GrabFocus();
    }

    /// <summary>
    ///     把弹窗摆到 NP 按钮旁边，并让箭头从弹窗指向按钮。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         布局规则（按优先级）：
    ///     </para>
    ///     <list type="number">
    ///         <item>
    ///             <b>默认在按钮右侧</b>。NP 条在屏幕左侧（HP 条下方），
    ///             右侧空间充足，且不会遮住玩家正在看的血条与手牌。
    ///         </item>
    ///         <item>
    ///             <b>右侧放不下就翻到左侧</b>（按钮靠近屏幕右缘时）。
    ///         </item>
    ///         <item>
    ///             弹窗<b>垂直居中对齐按钮</b>，并夹进屏幕内（留 <see cref="ScreenMargin" /> 边距）。
    ///         </item>
    ///     </list>
    ///     <para>
    ///         箭头用<b>指向性旋转</b>而不是写死角度：贴图默认朝向左下
    ///         （bearing ≈ 137.8°），这里按「弹窗侧边 → 按钮中心」的实际方位角求差值。
    ///         写死角度会在按钮上下移动时立刻指偏。
    ///     </para>
    ///     <para>
    ///         摆位<b>瞄箭头尖端而不是 pivot</b>：尖端离 pivot 有 143px，
    ///         瞄 pivot 会让箭头指到目标前方 143px 处。缩放取 0.5，
    ///         <see cref="GapHorizontal" /> 也据此放大，否则箭头会压在弹窗边框上。
    ///     </para>
    /// </remarks>
    private void LayoutTowards(Rect2 targetRect)
    {
        if (_popup == null || _arrow == null)
            return;

        var screen = GetViewport().GetVisibleRect().Size;

        // 尺寸必须在改 anchor 之前量：场景里 popup 用右下角 anchors + offset 定位，
        // 改成左上角后 offset 语义就变了，晚读会拿到塌陷尺寸导致摆位全错。
        var popupSize = _popup.Size;

        // 换成纯左上角定位模式，让 Position 直接等于屏幕坐标。
        // 四项 offset 必须显式按 popupSize 铺满：场景里 offset 是相对右下角的负值，
        // 光把 anchor 归零会让 TextureRect 按expand_mode（FIT_WIDTH_PROPORTIONAL）
        // 用贴图原生尺寸重算最小宽度，实测会从 579 被拉到 3012，弹窗大部分跑到屏外。
        _popup.AnchorLeft = 0f;
        _popup.AnchorTop = 0f;
        _popup.AnchorRight = 0f;
        _popup.AnchorBottom = 0f;
        _popup.OffsetLeft = 0f;
        _popup.OffsetTop = 0f;
        _popup.OffsetRight = popupSize.X;
        _popup.OffsetBottom = popupSize.Y;
        _popup.Position = Vector2.Zero;

        var targetCenter = targetRect.Position + targetRect.Size * 0.5f;

        // ① 默认放在目标右侧
        var x = targetRect.End.X + GapHorizontal;

        // ② 右侧越界就翻到左侧
        if (x + popupSize.X > screen.X - ScreenMargin)
            x = targetRect.Position.X - GapHorizontal - popupSize.X;

        // ③ 左侧也放不下（极窄屏）就硬夹回屏内
        x = Mathf.Clamp(x, ScreenMargin, Mathf.Max(ScreenMargin, screen.X - popupSize.X - ScreenMargin));

        // 垂直居中对齐目标，再夹进屏幕
        var y = targetCenter.Y - popupSize.Y * 0.5f;
        y = Mathf.Clamp(y, ScreenMargin, Mathf.Max(ScreenMargin, screen.Y - popupSize.Y - ScreenMargin));

        _popup.Position = new Vector2(x, y);

        LayoutArrow(targetCenter, new Vector2(x, y), popupSize);
    }

    /// <summary>
    ///     把箭头摆到弹窗朝向目标的那条边上，并旋转到正对目标中心。
    /// </summary>
    /// <param name="targetCenter">目标（NP 按钮）中心，屏幕坐标。</param>
    /// <param name="popupTopLeft">弹窗左上角，屏幕坐标。</param>
    /// <param name="popupSize">弹窗尺寸。</param>
    private void LayoutArrow(Vector2 targetCenter, Vector2 popupTopLeft, Vector2 popupSize)
    {
        if (_arrow == null)
            return;

        // 目标在弹窗左侧 ⇒ 箭头摆左边；反之摆右边。
        var targetIsLeft = targetCenter.X < popupTopLeft.X + popupSize.X * 0.5f;

        // 同样先量尺寸再铺 offset：TextureRect 换了 anchor 后会按 expand_mode 重算最小尺寸。
        var arrowSize = _arrow.Size;

        // 先按「箭头摆在弹窗侧边、中心对齐弹窗中心」估一个位置，只用来定朝向。
        var pivotGuess = new Vector2(
            targetIsLeft ? popupTopLeft.X : popupTopLeft.X + popupSize.X,
            popupTopLeft.Y + popupSize.Y * 0.5f
        );

        var delta = targetCenter - pivotGuess;
        var bearing = Mathf.RadToDeg(Mathf.Atan2(delta.Y, delta.X));
        _arrow.Rotation = Mathf.DegToRad(bearing - ArrowDefaultBearingDeg);
        _arrow.Scale = Vector2.One * ArrowScale;

        // Control 的变换顺序是「先按 pivot 平移 → 旋转 → 缩放 → 沿 -pivot 平移回来」，
        // 所以尖端在旋转缩放后的偏移 = ArrowTipOffset * Scale 再旋转 rotation。
        // 要尖端落在目标上，就得把 pivot 放在「目标 - 旋转后的尖端偏移」处，
        // 最后再减去 pivotOffset —— 因为 Position 记的是外框左上角。
        var rotatedTip = ArrowTipOffset * ArrowScale;
        rotatedTip = new Vector2(
            rotatedTip.X * Mathf.Cos(_arrow.Rotation) - rotatedTip.Y * Mathf.Sin(_arrow.Rotation),
            rotatedTip.X * Mathf.Sin(_arrow.Rotation) + rotatedTip.Y * Mathf.Cos(_arrow.Rotation)
        );

        // 同样切成纯左上角定位，让 Position 直接是屏幕坐标。
        _arrow.AnchorLeft = 0f;
        _arrow.AnchorTop = 0f;
        _arrow.AnchorRight = 0f;
        _arrow.AnchorBottom = 0f;
        _arrow.OffsetLeft = 0f;
        _arrow.OffsetTop = 0f;
        _arrow.OffsetRight = arrowSize.X;
        _arrow.OffsetBottom = arrowSize.Y;
        _arrow.Position = targetCenter - rotatedTip - _arrow.PivotOffset;
    }

    /// <summary>
    ///     确认按钮被按下。
    /// </summary>
    /// <remarks>
    ///     必须写成带 <c>NButton</c> 参数的私有重载来「遮蔽」基类的 <c>CloseFtue()</c>：
    ///     信号回调签名是 <c>Action&lt;NButton&gt;</c>，而基类那个是无参 protected 方法，
    ///     直接 <c>Callable.From((Action&lt;NButton&gt;)CloseFtue)</c> 会因为方法组转换找不到
    ///     匹配重载而编译失败（CS0123）。原版 <c>NCombatRewardFtue</c> 用的是同一手法。
    /// </remarks>
    private void OnConfirmReleased(NButton _)
    {
        CloseFtue();
    }
}