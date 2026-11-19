using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Ftue;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.TestSupport;

namespace Fgo.Scripts.UI;

/// <remarks>弹窗贴 NP 按钮、运行时摆位，由 <see cref="Create" /> 传入按钮矩形。</remarks>
public sealed partial class FgoNpBarFtue : NFtue
{
    private const string TitleKey = "FGO_NP_FTUE_TITLE";
    private const string DescriptionKey = "FGO_NP_FTUE_DESCRIPTION";

    private const float GapHorizontal = 110f;

    private const float ScreenMargin = 24f;

    private const float ArrowScale = 0.5f;

    private const float ArrowDefaultBearingDeg = 137.83f;

    private static readonly Vector2 ArrowTipOffset = new(-106f, 96f);

    private TextureRect? _arrow;
    private NButton? _confirmButton;
    private RichTextLabel? _description;

    private Label? _header;
    private Control? _popup;

    private Rect2 _targetRect;

    private static string ScenePath => $"{Entry.ResPath}/scenes/fgo_np_bar_ftue.tscn";

    /// <remarks>mod 场景不在主程序集的 <c>PreloadManager</c> 缓存里，须 <c>GD.Load</c>；路径写成常量以便导出时算进 pck 依赖。</remarks>
    public static FgoNpBarFtue? Create(Rect2 targetRect)
    {
        if (TestMode.IsOn)
            return null;

        var ftue = GD.Load<PackedScene>(ScenePath)
            .Instantiate<FgoNpBarFtue>();

        ftue._targetRect = targetRect;
        return ftue;
    }

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

        // 同步摆位（不用 deferred，否则首帧闪一帧原版右下角位置）。
        LayoutTowards(_targetRect);
        _confirmButton.GrabFocus();
    }

    /// <remarks>默认摆按钮右侧、放不下翻左；箭头按方位角旋转（瞄尖端而非 pivot，否则指偏）。</remarks>
    private void LayoutTowards(Rect2 targetRect)
    {
        if (_popup == null || _arrow == null)
            return;

        var screen = GetViewport().GetVisibleRect().Size;

        // 先量尺寸再改 anchor；offset 必须按 popupSize 铺满，否则 TextureRect 按 expand_mode 用贴图原生尺寸重算宽度，弹窗跑出屏外。
        var popupSize = _popup.Size;

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

        var x = targetRect.End.X + GapHorizontal;

        if (x + popupSize.X > screen.X - ScreenMargin)
            x = targetRect.Position.X - GapHorizontal - popupSize.X;

        x = Mathf.Clamp(x, ScreenMargin, Mathf.Max(ScreenMargin, screen.X - popupSize.X - ScreenMargin));

        var y = targetCenter.Y - popupSize.Y * 0.5f;
        y = Mathf.Clamp(y, ScreenMargin, Mathf.Max(ScreenMargin, screen.Y - popupSize.Y - ScreenMargin));

        _popup.Position = new Vector2(x, y);

        LayoutArrow(targetCenter, new Vector2(x, y), popupSize);
    }

    private void LayoutArrow(Vector2 targetCenter, Vector2 popupTopLeft, Vector2 popupSize)
    {
        if (_arrow == null)
            return;

        var targetIsLeft = targetCenter.X < popupTopLeft.X + popupSize.X * 0.5f;

        // 先量尺寸再铺 offset（TextureRect 换 anchor 会按 expand_mode 重算最小尺寸）。
        var arrowSize = _arrow.Size;

        var pivotGuess = new Vector2(
            targetIsLeft ? popupTopLeft.X : popupTopLeft.X + popupSize.X,
            popupTopLeft.Y + popupSize.Y * 0.5f
        );

        var delta = targetCenter - pivotGuess;
        var bearing = Mathf.RadToDeg(Mathf.Atan2(delta.Y, delta.X));
        _arrow.Rotation = Mathf.DegToRad(bearing - ArrowDefaultBearingDeg);
        _arrow.Scale = Vector2.One * ArrowScale;

        // 尖端偏移 = ArrowTipOffset * Scale 再旋转；pivot 放在「目标 - 该偏移 - PivotOffset」处，尖端才落在目标上。
        var rotatedTip = ArrowTipOffset * ArrowScale;
        rotatedTip = new Vector2(
            rotatedTip.X * Mathf.Cos(_arrow.Rotation) - rotatedTip.Y * Mathf.Sin(_arrow.Rotation),
            rotatedTip.X * Mathf.Sin(_arrow.Rotation) + rotatedTip.Y * Mathf.Cos(_arrow.Rotation)
        );

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

    // 带 NButton 参数以遮蔽基类无参 CloseFtue()：信号签名 Action<NButton>，直接转方法组会 CS0123。
    private void OnConfirmReleased(NButton _)
    {
        CloseFtue();
    }
}