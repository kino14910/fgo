using STS2RitsuLib.Settings;

namespace Fgo.Scripts;

public enum CharacterSkinId
{
    Chaldea = 0,
    HalloweenRoyalty = 1,
    SchoolUniformChapter2 = 2,
    MisakiHighUniform = 3,
    NewYear = 4,
    WhiteChristmas = 5,
    SummerStreet = 6,
    SounHighUniform = 7,
    FifthTrueElement = 8,
    SplendidNewYear = 9,
    TropicalSummer = 10,
    Fragment2004 = 11,
    MoonBackMemory = 12,
    MoonSeaMemory = 13,
    BrightSummer = 14,
    RoyalBrand = 15,
    GoldenCelebration = 16,
    ChaldeaExplorer = 17,
    WinterCasual = 18,
    MonteCristo = 19,
    CatArcueidBrunestud = 20,
    RomaniArchaman = 21,
    Guda = 22
}

[ModSettingsPage(Entry.ModId)]
// [ModSettingsSection("enemies", TitleLocKey = "FGO_SETTINGS_UI_ENEMIES.title")]
[ModSettingsSection("general", TitleLocKey = "FGO_SETTINGS_UI_GENERAL.title")]
public class FgoReflectedSettings
{
    // 各设置项的 entry Id（与上方 attribute 的首参一致），供选人页设置面板按 Id 定位绑定。
    public const string BaseNpPerCostEntryId = "baseNpPerCost";
    public const string EnablePadoruEntryId = "enablePadoru";
    public const string EnableNoCostNoblePhantasmEntryId = "enableNoCostNoblePhantasm";
    public const string OpenNobleDeckKeyEntryId = "openNobleDeckKey";
    public const string CharacterSkinEntryId = "characterSkin";

    private static bool _reflected;

    [ModSettingsIntSlider(BaseNpPerCostEntryId, "general", 0, 10,
        LabelLocKey = "FGO_SETTINGS_UI_BASE_NP_PER_COST.title",
        DescriptionLocKey = "FGO_SETTINGS_UI_BASE_NP_PER_COST.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global, DataKey = "base_np_per_cost")]
    public static int BaseNpPerCost { get; set; } = 5;

    [ModSettingsToggle(EnablePadoruEntryId, "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_PADORU.title",
        DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_PADORU.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global, DataKey = "enable_padoru")]
    public static bool EnablePadoru { get; set; } = false;

    // [ModSettingsToggle("enableFtue", "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FTUE.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FTUE.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableFtue { get; set; } = true;

    [ModSettingsToggle(EnableNoCostNoblePhantasmEntryId, "general",
        LabelLocKey = "FGO_SETTINGS_UI_ENABLE_NO_COST_NOBLE_PHANTASM.title",
        DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_NO_COST_NOBLE_PHANTASM.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global, DataKey = "enable_no_cost_noble_phantasm")]
    public static bool EnableNoCostNoblePhantasm { get; set; } = false;

    [ModSettingsChoice(CharacterSkinEntryId, "general",
        OptionLabels =
        [
            "迦勒底（默认）", "万圣节王室成员", "奏章2校服", "三咲高中校服", "新春装束",
            "白色圣诞", "夏日街头", "总耶高中校服", "第五真实元素环境用迦勒底制服", "华美的新年",
            "热带夏日", "2004年的碎片", "月之背面的记忆", "月之海的记忆", "明亮夏日",
            "王室品牌", "金色庆典", "迦勒底探险者", "冬日便装",
            "岩窟王　基督山", "猫姬", "罗曼", "人类恶"
        ],
        LabelLocKey = "FGO_SETTINGS_UI_CHARACTER_SKIN.title",
        DescriptionLocKey = "FGO_SETTINGS_UI_CHARACTER_SKIN.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global, DataKey = "character_skin")]
    public static CharacterSkinId CharacterSkin { get; set; } = CharacterSkinId.Chaldea;

    [ModSettingsKeyBinding(OpenNobleDeckKeyEntryId, "general", AllowModifierCombos = true, AllowModifierOnly = false,
        Label = "打开宝具牌堆 (Noble Deck)",
        Description = "战斗内按下此键打开 Noble Deck 宝具牌堆界面；可在设置中重新绑定。")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global, DataKey = "open_noble_deck_key")]
    public static string OpenNobleDeckKeyBinding { get; set; } = "N";

    /// <summary>
    ///     RitsuLib 的 [ModSettingsBinding] 静态镜像是「随设置 UI 读写时同步」的，游戏启动时不会把
    ///     已持久化的值回填到静态属性，重启后读到的一直是 C# 默认值。因此在开始使用前需显式调用本方法，
    ///     从已注册的设置页读取各绑定，把磁盘上的值回填到静态成员。
    /// </summary>
    public static void ReflectBoundValues()
    {
        if (_reflected) return;
        // 页面可能在主菜单预温时才注册，过早调用（如 mod 初始化阶段）会读不到，届时保持可重试。
        if (!ModSettingsRegistry.TryGetPage(Entry.ModId, Entry.ModId, out var page)) return;

        foreach (var section in page.Sections)
        foreach (var entry in section.Entries)
            if (entry is ToggleModSettingsEntryDefinition toggle) toggle.Binding.Read();
            else if (entry is IntSliderModSettingsEntryDefinition slider) slider.Binding.Read();
            else ReadEntryBinding(entry);

        _reflected = true;
    }

    /// <summary>
    ///     按 entry Id 取整数滑块绑定，供选人页等 UI 直接 Read/Write（Read 回填静态，Write 持久化 + 回填静态）。
    /// </summary>
    public static bool TryGetIntBinding(string entryId, out IModSettingsValueBinding<int> binding)
    {
        binding = null!;
        if (FindEntry(entryId) is IntSliderModSettingsEntryDefinition slider)
        {
            binding = slider.Binding;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     按 entry Id 取布尔开关绑定，供选人页等 UI 直接 Read/Write。
    /// </summary>
    public static bool TryGetToggleBinding(string entryId, out IModSettingsValueBinding<bool> binding)
    {
        binding = null!;
        if (FindEntry(entryId) is ToggleModSettingsEntryDefinition toggle)
        {
            binding = toggle.Binding;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     按 entry Id 取皮肤下拉(枚举)绑定，供选人页等 UI 直接 Read/Write（Write 会持久化 + 回填静态）。
    /// </summary>
    public static bool TryGetSkinBinding(out IModSettingsValueBinding<CharacterSkinId> binding)
    {
        binding = null!;
        if (FindEntry(CharacterSkinEntryId) is ChoiceModSettingsEntryDefinition<CharacterSkinId> choice)
        {
            binding = choice.Binding;
            return true;
        }

        return false;
    }

    private static void ReadEntryBinding(ModSettingsEntryDefinition entry)
    {
        var bindingProp = entry.GetType().GetProperty("Binding");
        var binding = bindingProp?.GetValue(entry);
        if (binding == null) return;
        var read = binding.GetType().GetMethod("Read", Type.EmptyTypes);
        read?.Invoke(binding, null);
    }

    private static ModSettingsEntryDefinition? FindEntry(string entryId)
    {
        if (!ModSettingsRegistry.TryGetPage(Entry.ModId, Entry.ModId, out var page)) return null;

        foreach (var section in page.Sections)
        foreach (var entry in section.Entries)
            if (string.Equals(entry.Id, entryId, StringComparison.OrdinalIgnoreCase))
                return entry;

        return null;
    }

    // [ModSettingsToggle("enableEnemies", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_ENEMIES.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_ENEMIES.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableEnemies { get; set; } = true;
    //
    // [ModSettingsToggle("enableEmiya", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_EMIYA.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_EMIYA.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableEmiya { get; set; } = true;
    //
    // [ModSettingsToggle("enableCalamityOfNorwich", "enemies",
    //     LabelLocKey = "FGO_SETTINGS_UI_ENABLE_CALAMITY_OF_NORWICH.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_CALAMITY_OF_NORWICH.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableCalamityOfNorwich { get; set; } = true;
    //
    // [ModSettingsToggle("enableCernunnos", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_CERNUNNOS.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_CERNUNNOS.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableCernunnos { get; set; } = true;
    //
    // [ModSettingsToggle("enableFaerieKnightGawain", "enemies",
    //     LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_GAWAIN.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_GAWAIN.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableFaerieKnightGawain { get; set; } = true;
    //
    // [ModSettingsToggle("enableFaerieKnightLancelot", "enemies",
    //     LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_LANCELOT.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_LANCELOT.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableFaerieKnightLancelot { get; set; } = true;
    //
    // [ModSettingsToggle("enableMoss", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_MOSS.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_MOSS.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableMoss { get; set; } = true;
    //
    // [ModSettingsToggle("enableQueenMorgan", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_QUEEN_MORGAN.title",
    //     DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_QUEEN_MORGAN.hover.desc")]
    // [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    // public static bool EnableQueenMorgan { get; set; } = true;
}