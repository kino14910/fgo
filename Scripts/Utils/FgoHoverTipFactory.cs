using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Cards.DynamicVars;

namespace Fgo.Scripts.Utils;

public static class FgoHoverTipFactory
{
    public static HoverTip FromNp()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_NP.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_NP.description"
        );

        return new HoverTip(title, desc);
    }

    public static HoverTip FromStar()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_STAR.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_STAR.description"
        );

        return new HoverTip(title, desc);
    }

    public static HoverTip FromNpBar()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_NP_BAR.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_NP_BAR.description"
        );

        var npRateVar = ModCardVars.Int(
            "NpRate",
            FgoConfigSync.NetworkBaseNpPerCost
        );
        desc.Add(npRateVar);

        return new HoverTip(title, desc);
    }

    public static HoverTip FromNpButton()
    {
        var title = new LocString(
            "cards",
            "FGO_CARD_RELEASE_NOBLE_PHANTASM.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_NP_BAR.description"
        );

        var npRateVar = ModCardVars.Int(
            "NpRate",
            FgoConfigSync.NetworkBaseNpPerCost
        );
        desc.Add(npRateVar);

        return new HoverTip(title, desc);
    }

    public static HoverTip FromNpSealed()
    {
        var title = new LocString(
            "cards",
            "FGO_CARD_RELEASE_NOBLE_PHANTASM.title"
        );
        var desc = new LocString(
            "gameplay_ui",
            "FGO_GAMEPLAY_UI_NOBLE_PHANTASM.text_0"
        );

        return new HoverTip(title, desc);
    }

    public static HoverTip FromForeigner()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_FOREIGNER.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_FOREIGNER.description"
        );

        return new HoverTip(title, desc);
    }

    public static HoverTip FromKnightOfChaldea()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_KNIGHT_OF_CHALDEA.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_KNIGHT_OF_CHALDEA.description"
        );

        return new HoverTip(title, desc);
    }

    public static HoverTip FromExploration()
    {
        var title = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_EXPLORATION.title"
        );
        var desc = new LocString(
            "static_hover_tips",
            "FGO_STATIC_HOVER_TIPS_EXPLORATION.description"
        );

        return new HoverTip(title, desc);
    }
}