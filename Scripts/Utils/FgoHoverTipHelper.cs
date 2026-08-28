using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Utils;

public class FgoHoverTipHelper
{
    private static readonly HoverTip _npHoverTip = BuildNpHoverTip();
    private static readonly HoverTip _starHoverTip = BuildStarHoverTip();
    private static readonly HoverTip _npBarHoverTip = BuildNpBarHoverTip();
    private static readonly HoverTip _foreignerHoverTip = BuildForeignerHoverTip();
    private static readonly HoverTip _knightOfChaldeaHoverTip = BuildKnightOfChaldeaHoverTip();

    public static HoverTip CreateNpHoverTip()
    {
        return _npHoverTip;
    }

    public static HoverTip CreateStarHoverTip()
    {
        return _starHoverTip;
    }

    public static HoverTip CreateNpBarHoverTip()
    {
        return _npBarHoverTip;
    }

    public static HoverTip CreateForeignerBarHoverTip()
    {
        return _foreignerHoverTip;
    }

    public static HoverTip CreateKnightOfChaldeaHoverTip()
    {
        return _knightOfChaldeaHoverTip;
    }

    private static HoverTip BuildNpHoverTip()
    {
        var title = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_NP.title");
        var desc = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_NP.description");

        return new HoverTip(title, desc);
    }

    private static HoverTip BuildStarHoverTip()
    {
        var title = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.title");
        var desc = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_STAR.description");

        return new HoverTip(title, desc);
    }

    private static HoverTip BuildNpBarHoverTip()
    {
        var title = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_NP_BAR.title");
        var desc = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_NP_BAR.description");
        var npRateVar = ModCardVars.Int("NpRate", FgoReflectedSettings.BaseNpPerCost);
        desc.Add(npRateVar);

        return new HoverTip(title, desc);
    }


    private static HoverTip BuildForeignerHoverTip()
    {
        var title = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_FOREIGNER.title");
        var desc = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_FOREIGNER.description");

        return new HoverTip(title, desc);
    }

    private static HoverTip BuildKnightOfChaldeaHoverTip()
    {
        var title = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_KNIGHT_OF_CHALDEA.title");
        var desc = new LocString("static_hover_tips", "FGO_STATIC_HOVER_TIPS_KNIGHT_OF_CHALDEA.description");

        return new HoverTip(title, desc);
    }
}