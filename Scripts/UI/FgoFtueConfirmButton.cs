using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace Fgo.Scripts.UI;

/// <summary>
///     mod 自有的 FTUE 确认按钮，等价于原版 <c>NFtueConfirmButton</c>。
/// </summary>
/// <remarks>
///     <para>
///         不能复用原版 <c>ftue_confirm_button.tscn</c>：其根节点挂的是主程序集
///         <c>NFtueConfirmButton.cs</c>，而 mod 的 pck 只导出 <c>res://Fgo/**</c> 与
///         <c>res://Scripts/**</c>，跨不到 <c>src/</c>（mod 项目无此目录）。
///     </para>
///     <para>
///         文字节点同理不能挂 <c>MegaLabel.cs</c>：导出时路径找不到，<c>script</c> 会静默变 null；
///         改用原生 <see cref="Label" />，字号自适应在 <see cref="FgoFtueText" /> 里用
///         <c>MegaLabelHelper</c> 的静态测量函数复刻。行为与原版一致：金色描边呼吸 +
///         聚焦放大 1.05 并借 <c>hsv.gdshader</c> 的 <c>v</c> 提亮，热键 <see cref="MegaInput.select" />。
///     </para>
/// </remarks>
public sealed partial class FgoFtueConfirmButton : NButton
{
    /// <summary>
    ///     按钮文字的字号上下限。
    /// </summary>
    /// <remarks>
    ///     <b>上限必须显式给，不能用 <see cref="FgoFtueText.SetAutoSizeText(Label,string,int,int)" /> 的默认值 100</b>：
    ///     原版 Label 挂 <c>MegaLabel</c> 且 <c>MaxFontSize = 24</c>，无另行声明上限；换成原生 Label 后
    ///     若沿用默认 100，短文本（如「Got it!」）会被 auto-size 撑到 40+ px，明显过大。
    /// </remarks>
    private const int FontMinSize = 12;

    private const int FontMaxSize = 20;
    private static readonly StringName VParam = "v";
    private ShaderMaterial? _hsv;

    private TextureRect? _outline;
    private Tween? _outlineTween;
    private Tween? _scaleTween;

    protected override string[] Hotkeys { get; } = [MegaInput.select];

    public override void _Ready()
    {
        // NButton._Ready 在被派生时会抛异常（要求调 ConnectSignals 而非 base._Ready），
        // 所以这里直接调 ConnectSignals，语义与原版 NFtueConfirmButton 一致。
        ConnectSignals();

        _outline = GetNodeOrNull<TextureRect>("Outline");
        _hsv = Material as ShaderMaterial;

        var label = GetNodeOrNull<Label>("%Label");
        if (label != null)
            FgoFtueText.SetAutoSizeText(
                label,
                new LocString("ftues", "CONFIRM_BUTTON").GetRawText(),
                FontMinSize,
                FontMaxSize
            );

        _outlineTween = CreateTween().SetLoops();
        _outlineTween.TweenProperty(_outline, "modulate:a", 1f, 0.6);
        _outlineTween.TweenProperty(_outline, "modulate:a", 0.25f, 0.6);
    }

    protected override void OnFocus()
    {
        base.OnFocus();

        _outlineTween?.Kill();
        if (_outline != null) _outline.Modulate = StsColors.gold;

        _scaleTween?.Kill();
        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", Vector2.One * 1.05f, 0.05)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        _scaleTween.TweenMethod(
            Callable.From<float>(UpdateShaderV),
            _hsv?.GetShaderParameter(VParam) ?? 1f,
            1.4f,
            0.05
        ).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);
    }

    protected override void OnUnfocus()
    {
        base.OnUnfocus();

        _scaleTween?.Kill();
        _scaleTween = CreateTween();
        _scaleTween.TweenProperty(this, "scale", Vector2.One, 0.5)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Expo);
        _scaleTween.TweenMethod(
            Callable.From<float>(UpdateShaderV),
            _hsv?.GetShaderParameter(VParam) ?? 1f,
            1f,
            0.5
        ).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Expo);

        _outlineTween?.Kill();
        _outlineTween = CreateTween().SetLoops();
        _outlineTween.TweenProperty(_outline, "modulate:a", 0.25f, 0.6);
        _outlineTween.TweenProperty(_outline, "modulate:a", 1f, 0.6)
            .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
    }

    private void UpdateShaderV(float value)
    {
        _hsv?.SetShaderParameter(VParam, value);
    }
}