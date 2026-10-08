using Godot;
using MegaCrit.Sts2.Core.RichTextTags;
using MegaCrit.Sts2.addons.mega_text;

namespace Fgo.Scripts.UI;

/// <summary>
///     FTUE 弹窗文字的辅助工具：字号自适应 + 原版 <c>[gold]</c>/<c>[red]</c> 等自定义 bbcode 效果安装。
/// </summary>
/// <remarks>
///     <para>
///         为什么需要它：FTUE 场景里的文字节点用引擎原生 <see cref="Label" /> /
///         <see cref="RichTextLabel" />，而不是原版的 <c>MegaLabel</c> / <c>MegaRichTextLabel</c>。
///         后者的 C# 脚本位于 <c>res://addons/mega_text/</c>，那是主程序集的路径；
///         mod 的 pck 只打包 <c>res://Fgo/**</c> 与 <c>res://Scripts/**</c>，
///         场景里引用 <c>res://addons/...</c> 会让 Godot 在导出时找不到文件、
///         <c>script</c> 静默变 null，节点退化成普通 Label（失去 auto-size 与 <c>[gold]</c>）。
///     </para>
///     <para>
///         但 <b>算法本身可以照搬</b>：<c>MegaLabelHelper</c> 是主程序集里的
///         <c>public static class</c>，mod 直接引用即可，无需复制源码；
///         <see cref="RichTextGold" /> 等 <c>RichTextEffect</c> 子类同理，
///         <c>new</c> 出来塞进 <c>CustomEffects</c> 即可。
///         语义与原版 <c>MegaRichTextLabel.InstallEffectsIfNeeded</c> 完全一致。
///     </para>
/// </remarks>
public static class FgoFtueText
{
    /// <summary>原版 <c>MegaRichTextLabel._textEffects</c> 的等价物（少了 <c>[fade]</c>/<c>[flyin]</c> 等与 FTUE 无关的动画标签）。</summary>
    private static readonly AbstractMegaRichTextEffect[] Effects =
    [
        new RichTextAqua(),
        new RichTextBlue(),
        new RichTextGold(),
        new RichTextGreen(),
        new RichTextOrange(),
        new RichTextPink(),
        new RichTextPurple(),
        new RichTextRed()
    ];

    private static readonly TextParagraph Paragraph = new();

    /// <summary>
    ///     给 <see cref="RichTextLabel" /> 装上原版那套 <c>[gold]</c>/<c>[red]</c>/… 自定义 bbcode。
    /// </summary>
    /// <remarks>
    ///     必须 <c>BbcodeEnabled = true</c> 才会生效（场景里已设）。
    ///     装完要重新 <c>ParseBbcode</c> 才会按新效果表解析已存在的文本，
    ///     所以本方法内部会调一次；调用方随后再赋 <c>Text</c> 也会自动重解析。
    /// </remarks>
    public static void InstallRichTextEffects(RichTextLabel label)
    {
        if (!label.BbcodeEnabled)
            return;

        if (label.CustomEffects.Count > 0)
            return;

        var array = new Godot.Collections.Array();
        foreach (var effect in Effects) array.Add(effect);

        label.CustomEffects = array;
        label.ParseBbcode(label.Text);
    }

    /// <summary>
    ///     设置文本并自动挑选不溢出的最大字号（<see cref="Label" /> 版）。
    /// </summary>
    /// <param name="label">目标节点，必须已设好 <c>font</c> 覆盖，否则测量没有基准字体。</param>
    /// <param name="text">要显示的文本。</param>
    /// <param name="minSize">允许的最小字号。</param>
    /// <param name="maxSize">允许的最大字号（文本很短时直接取这个）。</param>
    public static void SetAutoSizeText(Label label, string text, int minSize = 8, int maxSize = 100)
    {
        label.Text = text;

        var size = label.GetRect().Size;
        if (size.X <= 0f || size.Y <= 0f)
            return;

        var font = label.GetThemeFont(ThemeConstants.Label.Font, "Label");
        var lineSpacing = label.GetThemeConstant(ThemeConstants.Label.LineSpacing, "Label");
        var wrap = label.AutowrapMode != TextServer.AutowrapMode.Off;

        label.AddThemeFontSizeOverride(
            ThemeConstants.Label.FontSize,
            FitSize(text, font, maxSize, minSize, lineSpacing, wrap, size, label)
        );
    }

    /// <summary>
    ///     设置文本并自动挑选不溢出的最大字号（<see cref="RichTextLabel" /> 版，支持 bbcode）。
    /// </summary>
    /// <param name="label">目标节点，必须已设好 <c>normal_font</c> 覆盖。</param>
    /// <param name="text">要显示的文本，可含 <c>[gold]</c> 等自定义标签。</param>
    /// <param name="minSize">允许的最小字号。</param>
    /// <param name="maxSize">允许的最大字号（文本很短时直接取这个）。</param>
    public static void SetAutoSizeText(
        RichTextLabel label,
        string text,
        int minSize = 8,
        int maxSize = 100
    )
    {
        InstallRichTextEffects(label);
        label.Text = text;

        var size = label.GetRect().Size;
        if (size.X <= 0f || size.Y <= 0f)
            return;

        var font = label.GetThemeFont(ThemeConstants.RichTextLabel.NormalFont, "RichTextLabel");
        var lineSpacing = label.GetThemeConstant(ThemeConstants.RichTextLabel.LineSpacing, "RichTextLabel");
        var objs = MegaLabelHelper.ParseBbcode(text);
        var fit = FitRichSize(objs, font, maxSize, minSize, lineSpacing, size, label);

        label.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.NormalFontSize, fit);
        label.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.BoldFontSize, fit);
        label.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.BoldItalicsFontSize, fit);
        label.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.ItalicsFontSize, fit);
        label.AddThemeFontSizeOverride(ThemeConstants.RichTextLabel.MonoFontSize, fit);
        label.ParseBbcode(text);
    }

    /// <summary>节点尺寸变化后重算字号（对应原版 <c>_Notification(40 /* RESIZED */)</c> 的行为）。</summary>
    public static void Refit(Label label, int minSize = 8, int maxSize = 100)
    {
        var size = label.GetRect().Size;
        if (size.X <= 0f || size.Y <= 0f)
            return;

        var font = label.GetThemeFont(ThemeConstants.Label.Font, "Label");
        var lineSpacing = label.GetThemeConstant(ThemeConstants.Label.LineSpacing, "Label");
        var wrap = label.AutowrapMode != TextServer.AutowrapMode.Off;

        label.AddThemeFontSizeOverride(
            ThemeConstants.Label.FontSize,
            FitSize(label.Text, font, maxSize, minSize, lineSpacing, wrap, size, label)
        );
    }

    private static int FitSize(
        string text,
        Font font,
        int maxSize,
        int minSize,
        float lineSpacing,
        bool wrap,
        Vector2 rectSize,
        Label label
    )
    {
        if (!MegaLabelHelper.IsTooBig(Paragraph, text, font, maxSize, lineSpacing, wrap, rectSize))
            return maxSize;

        // 二分找出「放得下的最大字号」，与原版 MegaLabel.AdjustFontSize 同策略。
        var low = minSize;
        var high = maxSize;

        while (high >= low)
        {
            var mid = low + (high - low) / 2;

            if (MegaLabelHelper.IsTooBig(Paragraph, text, font, mid, lineSpacing, wrap, rectSize))
                high = mid - 1;
            else
                low = mid + 1;
        }

        return Mathf.Max(minSize, Mathf.Min(low, high));
    }

    private static int FitRichSize(
        System.Collections.Generic.List<MegaCrit.Sts2.Core.Entities.Text.BbcodeObject> objs,
        Font font,
        int maxSize,
        int minSize,
        float lineSpacing,
        Vector2 rectSize,
        RichTextLabel label
    )
    {
        if (!MegaLabelHelper.IsTooBig(Paragraph, objs, font, maxSize, lineSpacing, rectSize, true, true))
            return maxSize;

        var low = minSize;
        var high = maxSize;

        while (high >= low)
        {
            var mid = low + (high - low) / 2;

            if (MegaLabelHelper.IsTooBig(Paragraph, objs, font, mid, lineSpacing, rectSize, true, true))
                high = mid - 1;
            else
                low = mid + 1;
        }

        return Mathf.Max(minSize, Mathf.Min(low, high));
    }
}