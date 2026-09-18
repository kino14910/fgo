using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Fgo.Scripts.Fields;

/// <summary>
///     场地种类。
///     <para>
///         场地是「整张战场」的属性而非某个生物的状态，因此对所有单位（含敌我）统一生效。
///         与之相对，「某个单位身上着火」这类只影响单个目标的效果仍应写成 Power。
///     </para>
///     <para>
///         所有场地都是**有时限**的: 层数即剩余回合数，由施加方在 <c>FgoField.Add</c> 时给出
///         （每张卡各自决定自己的回合数），每回合开始时 -1，归零即消失。
///     </para>
///     由 <see cref="FgoField" /> 按战斗统一维护，<see cref="FgoFieldEffects" /> 负责结算。
/// </summary>
public enum FgoFieldId
{
    /// <summary>〔水边〕我方回合开始时，所有单位获得 <see cref="FgoFieldEffects.WatersideBlock" /> 点格挡。</summary>
    Waterside,

    /// <summary>〔燃烧〕我方回合结束时，所有单位受到 <see cref="FgoFieldEffects.BurningDamage" /> 点伤害。</summary>
    Burning,

    /// <summary>
    ///     〔黑暗〕我方回合开始时，所有单位获得 <see cref="FgoFieldEffects.DarknessBlindStacks" /> 层「盲目」。
    /// </summary>
    Darkness,

    /// <summary>
    ///     〔森林〕我方回合开始时，所有单位获得 <see cref="FgoFieldEffects.ForestThornsStacks" /> 层荆棘；
    ///     我方回合结束时，生命值低于 <see cref="FgoFieldEffects.ForestRegenHpRatio" /> 最大生命值的单位
    ///     获得 <see cref="FgoFieldEffects.ForestRegenAmount" /> 层再生。
    /// </summary>
    Forest,

    /// <summary>
    ///     〔都市〕<b>效果尚未确定（TODO）</b>——见 <see cref="FgoFieldEffects" /> 的说明。
    ///     当前只有「被施加 / 被查询」这条链路可用（如 <c>ThamesTroll</c> 的场地判定）。
    /// </summary>
    City,

    /// <summary>
    ///     〔阳光照射〕打出攻击牌时出牌者获得等同于剩余层数的「活力」；我方回合结束时
    ///     <b>己方单位</b>获得等量「活力」（唯一只对己方生效的场地）。
    ///     层数 = 剩余回合数。
    /// </summary>
    Sunlight,

    /// <summary>
    ///     〔虚数空间〕进入时把所有人的手牌换成额外手牌（航线规划 / 上浮吧鹦鹉螺号 / 虚数脱出）——
    ///     打出〔虚数脱出〕即打出鹦鹉螺的大冲角并退出虚数空间，随后原手牌回到手中。
    ///     也只有主动打出它才会提前离开，否则场地回合数归零后同样会退出。见 <c>FgoVoidHand</c>。
    /// </summary>
    ImaginarySpace
}

/// <summary>场地的展示信息（本地化键、图标与悬停提示）。</summary>
public static class FgoFieldIdExtensions
{
    private const string LocTable = "static_hover_tips";

    /// <summary>
    ///     场地本地化键前缀，形如 <c>FGO_STATIC_HOVER_TIPS_FIELD_SUNLIGHT</c>；
    ///     实际词条为 <c>{前缀}.title</c> / <c>{前缀}.description</c>。
    /// </summary>
    public static string LocKey(this FgoFieldId id) =>
        $"FGO_STATIC_HOVER_TIPS_FIELD_{id.ToString().ToUpperInvariant()}";

    /// <summary>场地图标资源路径，约定为 <c>res://Fgo/images/fields/{Id}.png</c>。</summary>
    public static string IconPath(this FgoFieldId id) => $"res://Fgo/images/fields/{id}.png";

    /// <summary>
    ///     场地是否「有时限」: 每回合开始时 -1 层，归零即消失。
    /// </summary>
    /// <remarks>
    ///     目前**所有场地**都由施加方给出回合数（层数 = 剩余回合数，见各卡牌的 <c>FgoField.Add</c> 调用），
    ///     因此一律递减。若将来出现「存在即持续」的场地，在这里排除它即可。
    /// </remarks>
    public static bool TicksDownEachTurn(this FgoFieldId id) => true;

    /// <summary>
    ///     加载场地图标（按需缓存）。资源尚未被 Godot 导入或缺失时返回 null，
    ///     调用方需要按无图标渲染，不能假定一定拿得到。
    /// </summary>
    public static Texture2D? Icon(this FgoFieldId id)
    {
        if (IconCache.TryGetValue(id, out var cached)) return cached;

        var path = id.IconPath();
        var icon = ResourceLoader.Exists(path) ? GD.Load<Texture2D>(path) : null;
        IconCache[id] = icon;
        return icon;
    }

    /// <summary>构造该场地的悬停提示（带图标），用于卡牌的 <c>AdditionalHoverTips</c> 与 HUD。</summary>
    public static HoverTip ToHoverTip(this FgoFieldId id) =>
        new(new LocString(LocTable, $"{id.LocKey()}.title"),
            new LocString(LocTable, $"{id.LocKey()}.description"),
            id.Icon());

    /// <summary>卡面与 HUD 上显示的场地名称。</summary>
    public static string ToTitle(this FgoFieldId id) =>
        new LocString(LocTable, $"{id.LocKey()}.title").GetFormattedText();

    private static readonly Dictionary<FgoFieldId, Texture2D?> IconCache = [];
}
