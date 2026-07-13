using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Entities.UI;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using System.Reflection;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Fgo.Scripts.Character;

public class NobleCardPool : TypeListCardPoolModel
{
    // 根据你使用的卡框决定使用哪个Material
    // private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateReplaceHueShaderMaterial(0.5f, 0.5f, 1f); // 如果你使用原版卡框，使用这个直接替换色调。
    // private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateRgbShaderMaterial(0.5f, 0.5f, 1f); // 使用原版卡框替换色调。除非你的版本没有CreateReplaceHueShaderMaterial函数，否则应使用上面那种
    private static readonly Material?
        _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial(); // 如果你是自定义卡框，使用这个

    // 卡池的ID。必须唯一防撞车。
    public override string Title => "noble";
    public override string EnergyColorName => "noble";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://Fgo/images/ui/energy_fgo.png";

    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Fgo/images/ui/energy_noble_big.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);

    // 能量表盘文字轮廓颜色
    public override Color EnergyOutlineColor => new(0.5f, 0.5f, 1f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}

/// <summary>
/// 为自定义稀有度 NoblePhantasm 提供排序值。
/// </summary>
[HarmonyPatch(typeof(NCardGrid), "GetCardRarityComparisonValue")]
static class NobleRaritySortPatch
{
    [HarmonyPrefix]
    static bool HandleNoblePhantasmRarity(CardModel a, ref int __result)
    {
        if (a.Pool is NobleCardPool && a.Rarity == FgoEnums.NoblePhantasm)
        {
            __result = 11;
            return false;
        }
        return true;
    }
}

/// <summary>
/// 为 NobleCardPool 添加筛选按钮。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), nameof(NCardLibrary._Ready))]
[HarmonyPriority(Priority.High)]
static class NoblePoolFilterPatch
{
    [HarmonyPostfix]
    static void AddNobleFilter(NCardLibrary __instance,
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> ____poolFilters,
        Dictionary<NCardRarityTickbox, Func<CardModel, bool>> ____rarityFilters)
    {
        if (____poolFilters.Count == 0) return;
        if (____poolFilters.Last().Key.GetParentControl() is not GridContainer parent) return;

        var filter = CreateFilter();
        parent.AddChild(filter);

        ____poolFilters.Add(filter, c => c.Pool is NobleCardPool);

        var updateMethod = AccessTools.Method(typeof(NCardLibrary), "UpdateCardPoolFilter")!;
        filter.Connect(NCardPoolFilter.SignalName.Toggled,
            Callable.From<NCardPoolFilter>(f => updateMethod.Invoke(__instance, [f])));

        var lastHoveredField = AccessTools.DeclaredField(typeof(NCardLibrary), "_lastHoveredControl");
        if (lastHoveredField != null)
        {
            filter.Connect(Control.SignalName.FocusEntered,
                Callable.From(() => lastHoveredField.SetValue(__instance, filter)));
        }
    }

    static NCardPoolFilter CreateFilter()
    {
        var filter = new NCardPoolFilter
        {
            Name = "FILTER-Noble",
            Size = new Vector2(64, 64),
            CustomMinimumSize = new Vector2(64, 64),
            FocusMode = Control.FocusModeEnum.All
        };

        var tex = PreloadManager.Cache.GetTexture2D("res://Fgo/images/charui/character_icon_fgo.png");

        var image = new TextureRect
        {
            Name = "Image",
            Texture = tex,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(56, 56),
            Position = new Vector2(4, 4),
            Scale = new Vector2(0.9f, 0.9f),
            PivotOffset = new Vector2(28, 28),
        };

        var shadow = new TextureRect
        {
            Name = "Shadow",
            Texture = tex,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(56, 56),
            Position = new Vector2(4, 3),
            PivotOffset = new Vector2(28, 28),
            ShowBehindParent = true,
            Modulate = Colors.Black with { A = 0.25f },
        };

        image.AddChild(shadow);

        var reticle = PreloadManager.Cache
            .GetScene(SceneHelper.GetScenePath("ui/selection_reticle"))
            .Instantiate<NSelectionReticle>();
        reticle.Name = "SelectionReticle";
        reticle.UniqueNameInOwner = true;

        filter.AddChild(image);
        image.Owner = filter;
        filter.AddChild(reticle);
        reticle.Owner = filter;

        return filter;
    }
}

/// <summary>
/// 使 Noble 池被选中时禁用稀有度筛选。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), "UpdateCardPoolFilter")]
static class NobleRarityPatch
{
    private static MethodInfo? _updateFilterMethod;

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Low)]
    static bool HandleNobleRarity(
        NCardPoolFilter filter,
        NCardLibrary __instance,
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> ____poolFilters,
        Dictionary<NCardRarityTickbox, Func<CardModel, bool>> ____rarityFilters)
    {
        var noble = ____poolFilters.Keys.FirstOrDefault(k => k.Name == "FILTER-Noble");
        if (noble == null || !noble.IsSelected) return true;
        if (filter != noble) return true;

        foreach (var key in ____poolFilters.Keys)
            if (key != noble) key.IsSelected = false;

        foreach (var tickbox in ____rarityFilters.Keys)
            tickbox.Disable();

        _updateFilterMethod ??= AccessTools.Method(typeof(NCardLibrary), "UpdateFilter", [typeof(bool)]);
        _updateFilterMethod?.Invoke(__instance, [false]);

        return false;
    }
}

/// <summary>
/// 在 NCard.Reload() 后为 NobleCardPool 卡牌强制应用全画幅渲染。
/// 不依赖 Rarity 属性（ModCardTemplate 可能重写了它），直接操作 NCard 的私有子节点。
/// </summary>
[HarmonyPatch(typeof(NCard), "Reload")]
static class NobleCardFullArtPatch
{
    private static readonly FieldInfo ModelField = AccessTools.Field(typeof(NCard), "_model");
    private static readonly FieldInfo AncientPortraitField = AccessTools.Field(typeof(NCard), "_ancientPortrait");
    private static readonly FieldInfo AncientBorderField = AccessTools.Field(typeof(NCard), "_ancientBorder");
    private static readonly FieldInfo AncientTextBgField = AccessTools.Field(typeof(NCard), "_ancientTextBg");
    private static readonly FieldInfo AncientBannerField = AccessTools.Field(typeof(NCard), "_ancientBanner");
    private static readonly FieldInfo AncientHighlightField = AccessTools.Field(typeof(NCard), "_ancientHighlight");
    private static readonly FieldInfo PortraitField = AccessTools.Field(typeof(NCard), "_portrait");
    private static readonly FieldInfo PortraitBorderField = AccessTools.Field(typeof(NCard), "_portraitBorder");
    private static readonly FieldInfo FrameField = AccessTools.Field(typeof(NCard), "_frame");
    private static readonly FieldInfo BannerField = AccessTools.Field(typeof(NCard), "_banner");
    private static readonly FieldInfo PortraitCanvasGroupField = AccessTools.Field(typeof(NCard), "_portraitCanvasGroup");

    private static readonly string _canvasGroupMaskMaterialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";

    [HarmonyPostfix]
    static void ForceFullArt(NCard __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;

        var model = (CardModel?)ModelField.GetValue(__instance);
        if (model?.Pool is not NobleCardPool) return;

        var ancientPortrait = (TextureRect?)AncientPortraitField.GetValue(__instance);
        var ancientBorder = (TextureRect?)AncientBorderField.GetValue(__instance);
        var ancientTextBg = (TextureRect?)AncientTextBgField.GetValue(__instance);
        var ancientBanner = (Control?)AncientBannerField.GetValue(__instance);
        var ancientHighlight = (TextureRect?)AncientHighlightField.GetValue(__instance);
        var portrait = (TextureRect?)PortraitField.GetValue(__instance);
        var portraitBorder = (TextureRect?)PortraitBorderField.GetValue(__instance);
        var frame = (TextureRect?)FrameField.GetValue(__instance);
        var banner = (TextureRect?)BannerField.GetValue(__instance);
        var portraitCanvasGroup = (CanvasGroup?)PortraitCanvasGroupField.GetValue(__instance);

        // 显示 ancient 全画幅元素
        if (ancientPortrait != null)
        {
            ancientPortrait.Visible = true;
            ancientPortrait.Texture = model.Portrait;
            ancientPortrait.Material = null;
        }

        if (ancientBorder != null)
            ancientBorder.Visible = true;

        if (ancientTextBg != null)
        {
            ancientTextBg.Visible = true;
            ancientTextBg.Texture = LoadAncientTextBg(model);
        }

        if (ancientHighlight != null)
            ancientHighlight.Visible = true;

        // 隐藏普通元素
        if (portrait != null)
            portrait.Visible = false;

        if (portraitBorder != null)
            portraitBorder.Visible = false;

        if (frame != null)
            frame.Visible = false;

        if (banner != null)
            banner.Visible = false;

        // 隐藏 ancient banner（横幅由卡面本身展示）
        if (ancientBanner != null)
            ancientBanner.Visible = false;

        // 设置 portrait canvas group 的遮罩 material（与原生 Ancient 渲染一致）
        if (portraitCanvasGroup != null)
            portraitCanvasGroup.Material = PreloadManager.Cache.GetMaterial(_canvasGroupMaskMaterialPath);
    }

    private static Texture2D LoadAncientTextBg(CardModel model)
    {
        var cardType = model.Type switch
        {
            CardType.None or CardType.Status or CardType.Curse => CardType.Skill,
            _ => model.Type
        };
        var path = ImageHelper.GetImagePath(
            $"atlases/compressed.sprites/card_template/ancient_card_text_bg_{cardType.ToString().ToLowerInvariant()}.tres");
        return ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
    }
}

[HarmonyPatch(typeof(NCard), "GetTitleLabelOutlineColor")]
static class NobleTitleOutlinePatch
{
    static bool Prefix(NCard __instance, ref Color __result)
    {
        var model = AccessTools.Field(typeof(NCard), "_model")?.GetValue(__instance) as CardModel;
        if (model?.Pool is NobleCardPool)
        {
            __result = StsColors.cardTitleOutlineSpecial;
            return false;
        }
        return true;
    }
}