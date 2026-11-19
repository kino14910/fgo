using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Fgo.Scripts.Fields;

/// <summary>场地 = 整张战场的属性，对所有单位（含敌我）统一生效；单位级效果应写成 Power。层数 = 剩余回合数，每回合开始 -1、归零消失。</summary>
public enum FgoFieldId
{
    Waterside,

    Burning,

    Darkness,

    Forest,

    City,

    /// <summary>唯一只对己方生效的场地。</summary>
    Sunlight
}

public static class FgoFieldIdExtensions
{
    private const string LocTable = "static_hover_tips";

    private static readonly Dictionary<FgoFieldId, Texture2D?> IconCache = [];

    /// <summary>形如 <c>FGO_STATIC_HOVER_TIPS_FIELD_SUNLIGHT</c>，词条为 <c>.title</c> / <c>.description</c>。</summary>
    public static string LocKey(this FgoFieldId id)
    {
        return $"FGO_STATIC_HOVER_TIPS_FIELD_{id.ToString().ToUpperInvariant()}";
    }

    public static string IconPath(this FgoFieldId id)
    {
        return $"res://Fgo/images/fields/{id}.png";
    }

    /// <remarks>目前所有场地都限时，恒为 true；将来若有常驻场地在此排除。</remarks>
    public static bool TicksDownEachTurn(this FgoFieldId id)
    {
        return true;
    }

    /// <summary>缺失时返回 null，调用方须按无图标渲染。</summary>
    public static Texture2D? Icon(this FgoFieldId id)
    {
        if (IconCache.TryGetValue(id, out var cached)) return cached;

        var path = id.IconPath();
        var icon = ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
        IconCache[id] = icon;
        return icon;
    }

    public static HoverTip ToHoverTip(this FgoFieldId id)
    {
        return new HoverTip(new LocString(LocTable, $"{id.LocKey()}.title"),
            new LocString(LocTable, $"{id.LocKey()}.description"),
            id.Icon());
    }

    public static string ToTitle(this FgoFieldId id)
    {
        return new LocString(LocTable, $"{id.LocKey()}.title").GetFormattedText();
    }
}