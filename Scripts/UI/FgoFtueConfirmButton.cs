using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.addons.mega_text;

namespace Fgo.Scripts.UI;

/// <summary>
///     mod 自有的 FTUE 确认按钮，等价于原版 <c>NFtueConfirmButton</c>。
/// </summary>
/// <remarks>
///     <para>
///         为什么要自己写而不能直接用原版 <c>ftue_confirm_button.tscn</c>：
///         原版场景的根节点挂着 <c>res://src/Core/Nodes/Ftue/NFtueConfirmButton.cs</c>，
///         那是主程序集里的 C# 类型，而 mod 的 pck 导出只打包 <c>res://Fgo/**</c> 与
///         <c>res://Scripts/**</c>（csproj 里 <c>Content Include="Fgo\**"</c>），
///         跨不过去（mod 项目 <c>D:\projects\fgo</c> 里根本没有 <c>src/</c> 目录）。
///     </para>
///     <para>
///         同理按钮里的文字节点也不能挂 <c>res://addons/mega_text/MegaLabel.cs</c>：
///         导出时 Godot 找不到该路径，<c>script</c> 会静默变 null。
///         场景里改用引擎原生 <see cref="Label" />，字号自适应在
///         <see cref="FgoFtueText" /> 里用原版 <c>MegaLabelHelper</c> 的静态测量函数复刻。
///     </para>
///     <para>
///         其余行为与原版保持一致：金色描边呼吸循环 + 聚焦时放大 1.05 倍并提亮
///         （靠 <c>hsv.gdshader</c> 的 <c>v</c> 参数），手柄热键走
///         <see cref="MegaInput.select" />。
///     </para>
/// </remarks>
public sealed partial class FgoFtueConfirmButton : NButton
{
    private static readonly StringName VParam = "v";

    /// <summary>
    ///     按钮文字的字号上下限。
    /// </summary>
    /// <remarks>
    ///     <b>上限必须显式给，不能用 <see cref="FgoFtueText.SetAutoSizeText(Label,string,int,int)" /> 的默认值 100</b>：
    ///     原版这个 Label 挂的是 <c>MegaLabel</c> 且 <c>MaxFontSize = 24</c>，场景里没有另行声明上限。
    ///     换成原生 Label 后若沿用默认 100，auto-size 二分会在「按钮实际能放下」的范围内
    ///     一直撑到最大，「Got it!」这种短文本会顶到 40+ px，视觉上明显过大。
    /// </remarks>
    private const int FontMinSize = 12;

    private const int FontMaxSize = 20;

    private TextureRect? _outline;
    private ShaderMaterial? _hsv;
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