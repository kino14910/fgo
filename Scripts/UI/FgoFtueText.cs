using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Text;
using MegaCrit.Sts2.Core.RichTextTags;
using Array = Godot.Collections.Array;

namespace Fgo.Scripts.UI;

/// <summary>
///     FTUE 弹窗文字的辅助工具：字号自适应 + 原版 <c>[gold]</c>/<c>[red]</c> 等自定义 bbcode 效果安装。
/// </summary>
public static class FgoFtueText
{
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
    public static void InstallRichTextEffects(RichTextLabel label)
    {
        if (!label.BbcodeEnabled)
            return;

        if (label.CustomEffects.Count > 0)
            return;

        var array = new Array();
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

        // 二分末态 low 已溢出、high 为最后可行解，取 min 才是最大不溢出字号。
        return Mathf.Max(minSize, Mathf.Min(low, high));
    }

    private static int FitRichSize(
        List<BbcodeObject> objs,
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

        // 二分末态 low 已溢出、high 为最后可行解，取 min 才是最大不溢出字号。
        return Mathf.Max(minSize, Mathf.Min(low, high));
    }
}