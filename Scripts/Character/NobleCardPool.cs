using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using MegaCrit.Sts2.Core.Entities.UI;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
using System.Collections;
using System.Reflection;
using Fgo.Scripts.Utils;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Fgo.Scripts.Character;

/// <summary>
/// 注册为共享卡池：会被纳入 <see cref="ModelDb.AllSharedCardPools"/>，
/// 进而进入 <see cref="ModelDb.AllCardPools"/> 和 <see cref="ModelDb.AllCards"/>。
/// 只有进入 AllCards，卡牌图书馆的 NCardLibraryGrid._allCards 才会包含 Noble 卡，
/// 点击 Noble 筛选按钮时才能显示出卡牌。
/// </summary>
[RegisterSharedCardPool]
public class NobleCardPool : TypeListCardPoolModel
{
    private static readonly Material? _poolFrameMaterial =
        MaterialUtils.CreateUnmodulatedHsvShaderMaterial();

    public override string Title => "noble";
    public override string EnergyColorName => "noble";
    public override string? TextEnergyIconPath => "res://Fgo/images/ui/energy_fgo.png";
    public override string? BigEnergyIconPath => "res://Fgo/images/ui/energy_noble_big.png";
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);
    public override Color EnergyOutlineColor => new Color("D4AF37"); // 金色，与 vanilla 池用 hex 字符串一致
    public override Material? PoolFrameMaterial => _poolFrameMaterial;
    public override bool IsColorless => false;
}

/// <summary>
/// 为自定义稀有度 NoblePhantasm 提供排序值。
/// 通过 RitsuLib 的 IPatchMethod 模式注册（见 Entry.Init）。
/// </summary>
public sealed class NobleRaritySortPatch : IPatchMethod
{
    public static string PatchId => "fgo.noble.rarity_sort";
    public static string Description => "Sort NoblePhantasm-rarity Noble cards above all others";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCardGrid>("GetCardRarityComparisonValue")];

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
/// NobleCardPool 的卡牌图书馆筛选按钮注册。
/// 实际由 Entry.Init 中调用 RitsuLib 的 ModContentRegistry.For(ModId)
///     .RegisterCardLibraryCompendiumSharedPoolFilter&lt;NobleCardPool&gt;(...)
/// 完成注册 —— RitsuLib 的 <see cref="STS2RitsuLib.Scaffolding.Characters.Patches.CardLibraryCompendiumPatch"/>
/// 会在 NCardLibrary._Ready 的 postfix 里自动：
///   1. 找一个 vanilla pool filter 作模板（保证 HSV ShaderMaterial、Image 子节点、
///      %SelectionReticle 子节点完整）
///   2. 实例化新按钮、替换图标
///   3. 插入到 strip 容器
///   4. 注册到 vanilla 的 _poolFilters 字典（谓词使用 pool.AllCardIds.Contains(c.Id)）
///   5. 连接 Toggled → UpdateCardPoolFilter，连接 FocusEntered → _lastHoveredControl
///
/// 自己写 patch 会被两个问题困住：（1）vanilla 不 PatchAll(Fgo.dll)、Ritsu 不扫
/// [HarmonyPatch] 特性，所以属性式 patch 永远不会被注册；（2）就算手动注册也会
/// 和 Ritsu 的同名 postfix 冲突，操作 _poolFilters 顺序不可控。
/// </summary>
internal static class NoblePoolFilterRegistration
{
    public const string StableId = "Noble";
    public const string IconTexturePath = "res://Fgo/images/charui/character_icon_fgo.png";
}

/// <summary>
///     隐藏 Noble 卡的 Ancient 横幅。
///     Ritsu 的 CardVisualStylePatch.Postfix 会在 NCard.Reload 后调用 ApplyVisualStyle，
///     强制 %AncientBanner.Visible = true。这里用 Priority.Low 确保在 Ritsu 之后执行，
///     把横幅藏掉。
///     采用 Ritsu 的 IPatchMethod 模式：通过 RitsuLibFramework.CreatePatcher +
///     RegisterPatch&lt;NobleCardHideBannerPatch&gt;() 注册（见 Entry.Init）。
///     NCard.Reload 是 private，因此 GetTargets 用字符串字面量而非 nameof。
/// </summary>
public sealed class NobleCardHideBannerPatch : IPatchMethod
{
    public static string PatchId => "fgo.noble.hide_ancient_banner";
    public static string Description => "Hide %AncientBanner for Noble pool cards using Ancient visual style";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCard>("Reload")];

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    public static void Postfix(NCard __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;
        if (__instance.Model?.Pool is not NobleCardPool) return;

        if (__instance.GetNodeOrNull<CanvasItem>("%AncientBanner") is { } banner)
            banner.Visible = false;
    }
}

/// <summary>
/// 为 Noble 卡提供标题描边颜色。
///
/// Noble 卡使用动态注册的稀有度 NoblePhantasm，vanilla 的 GetTitleLabelOutlineColor
/// switch 不认识它，会走到 default 分支抛 ArgumentOutOfRangeException。本 patch 拦截
/// Noble 卡的调用，返回透明色（alpha=0），让标题描边完全消失。
///
/// 通过 RitsuLib 的 IPatchMethod 模式注册（见 Entry.Init）。
/// </summary>
public sealed class NobleTitleOutlinePatch : IPatchMethod
{
    public static string PatchId => "fgo.noble.title_outline";
    public static string Description => "Transparent title outline for Noble pool cards (hides the outline)";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCard>("GetTitleLabelOutlineColor")];

    static bool Prefix(NCard __instance, ref Color __result)
    {
        var model = AccessTools.Field(typeof(NCard), "_model")?.GetValue(__instance) as CardModel;
        if (model?.Pool is NobleCardPool)
        {
            // 透明色（alpha=0）：完全去掉标题文字描边
            __result = new Color(0.42f, 0.36f, 0.08f);
            return false;
        }
        return true;
    }
}

/// <summary>
/// 当 Noble 池筛选按钮被选中时，禁用稀有度勾选框（视觉+逻辑），与 misc/ancients 行为一致。
///
/// vanilla 的 UpdateCardPoolFilter 用以下判断决定是否禁用稀有度勾选框：
///   if (key.IsSelected && key != _miscPoolFilter && key != _ancientsFilter)
///       flag = false;  // 非 misc/ancients 被选中 → flag=false → Enable() 勾选框
///
/// Noble 既不是 misc 也不是 ancients，所以 vanilla 默认会 Enable() 勾选框。
/// 我们在 Postfix 中检查触发本次调用的 filter 是否是 Noble filter 且被选中，
/// 如果是则通过反射获取 _rarityFilters 并 Disable() 所有勾选框。
///
/// 不依赖 Harmony 字段注入（___fieldName），因为字段注入在某些环境下会静默注入 null。
/// 改用方法参数 filter（参数注入更可靠）+ 反射获取私有字段。
/// </summary>
public sealed class NobleRarityLockPatch : IPatchMethod
{
    public static string PatchId => "fgo.noble.rarity_lock";
    public static string Description => "Disable rarity tickboxes when Noble pool filter is selected (like misc/ancients)";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCardLibrary>("UpdateCardPoolFilter")];

    private static readonly FieldInfo? RarityFiltersField =
        AccessTools.Field(typeof(NCardLibrary), "_rarityFilters");

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)]
    static void Postfix(NCardLibrary __instance, NCardPoolFilter filter)
    {
        // filter 是触发本次 UpdateCardPoolFilter 的按钮（方法参数注入，比字段注入可靠）
        // 如果不是 Noble filter 或未被选中，不做任何事
        if (filter is null || filter.Name != "MOD_FILTER_SHARED_Noble" || !filter.IsSelected)
            return;

        GD.Print($"[Noble] Noble filter 被选中，禁用稀有度勾选框");

        // 反射获取 _rarityFilters（不依赖 Harmony 字段注入）
        if (RarityFiltersField?.GetValue(__instance) is not IDictionary rarityFilters)
        {
            GD.PrintErr("[Noble] 无法获取 _rarityFilters 字段");
            return;
        }

        foreach (var key in rarityFilters.Keys)
        {
            if (key is NCardRarityTickbox tickbox)
                tickbox.Disable();
        }
    }
}

/// <summary>
/// 替换 Noble filter 的筛选谓词，确保点击 Noble 按钮能正确筛选出 Noble 卡。
///
/// Ritsu 的 RegisterCardLibraryCompendiumSharedPoolFilter 默认用谓词
/// pool.AllCardIds.Contains(c.Id)。但 AllCardIds 依赖 ModHelper.ConcatModelsFromMods
/// 正确合并 [RegisterCard] 注册的卡，时序或缓存问题可能导致其为空。
/// 这里在 Ritsu 的 _Ready Postfix 之后运行（Priority.Low），把谓词替换为更健壮的
/// c.Pool is NobleCardPool —— 直接按池类型筛选，不依赖 AllCardIds。
/// </summary>
public sealed class NoblePoolPredicatePatch : IPatchMethod
{
    public static string PatchId => "fgo.noble.pool_predicate";
    public static string Description => "Replace Noble pool filter predicate with c.Pool is NobleCardPool for robust filtering";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [PatchTarget.Method<NCardLibrary>(nameof(NCardLibrary._Ready))];

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Low)] // 确保在 Ritsu 的 CardLibraryCompendiumPatch 之后运行
    static void Postfix(Dictionary<NCardPoolFilter, Func<CardModel, bool>> ___poolFilters)
    {
        // 找到 Ritsu 创建的 Noble filter
        var nobleFilter = ___poolFilters.Keys
            .FirstOrDefault(k => k.Name == "MOD_FILTER_SHARED_Noble");
        if (nobleFilter == null)
        {
            GD.PrintErr("[Noble] 未找到 Noble 筛选按钮，无法替换谓词");
            return;
        }

        // 替换谓词为按池类型筛选（比 AllCardIds.Contains 更健壮）
        var newPredicate = (Func<CardModel, bool>)(c => c.Pool is NobleCardPool);
        ___poolFilters[nobleFilter] = newPredicate;
        GD.Print("[Noble] 已替换 Noble 筛选谓词为 c.Pool is NobleCardPool");
    }
}
