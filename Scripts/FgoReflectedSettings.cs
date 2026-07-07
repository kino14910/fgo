using STS2RitsuLib.Settings;

namespace Fgo.Scripts;

[ModSettingsPage(Entry.ModId)]
[ModSettingsSection("enemies", TitleLocKey = "FGO_SETTINGS_UI_ENEMIES.title")]
[ModSettingsSection("general", TitleLocKey = "FGO_SETTINGS_UI_GENERAL.title")]
public class FgoReflectedSettings
{
    [ModSettingsIntSlider("baseNpPerCost", "general", 0, 10, LabelLocKey = "FGO_SETTINGS_UI_BASE_NP_PER_COST.title", DescriptionLocKey = "FGO_SETTINGS_UI_BASE_NP_PER_COST.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static int BaseNpPerCost { get; set; } = 5;

    [ModSettingsToggle("enableColorlessCards", "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_COLORLESS_CARDS.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_COLORLESS_CARDS.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableColorlessCards { get; set; } = true;

    [ModSettingsToggle("enablePadoru", "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_PADORU.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_PADORU.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnablePadoru { get; set; } = false;

    [ModSettingsToggle("enableFtue", "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FTUE.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FTUE.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableFtue { get; set; } = true;

    [ModSettingsToggle("enableNoCostNoblePhantasm", "general", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_NO_COST_NOBLE_PHANTASM.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_NO_COST_NOBLE_PHANTASM.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableNoCostNoblePhantasm { get; set; } = false;

    [ModSettingsToggle("enableEnemies", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_ENEMIES.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_ENEMIES.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableEnemies { get; set; } = true;

    [ModSettingsToggle("enableEmiya", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_EMIYA.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_EMIYA.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableEmiya { get; set; } = true;

    [ModSettingsToggle("enableCalamityOfNorwich", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_CALAMITY_OF_NORWICH.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_CALAMITY_OF_NORWICH.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableCalamityOfNorwich { get; set; } = true;

    [ModSettingsToggle("enableCernunnos", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_CERNUNNOS.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_CERNUNNOS.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableCernunnos { get; set; } = true;

    [ModSettingsToggle("enableFaerieKnightGawain", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_GAWAIN.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_GAWAIN.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableFaerieKnightGawain { get; set; } = true;

    [ModSettingsToggle("enableFaerieKnightLancelot", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_LANCELOT.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_FAERIE_KNIGHT_LANCELOT.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableFaerieKnightLancelot { get; set; } = true;

    [ModSettingsToggle("enableMoss", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_MOSS.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_MOSS.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableMoss { get; set; } = true;

    [ModSettingsToggle("enableQueenMorgan", "enemies", LabelLocKey = "FGO_SETTINGS_UI_ENABLE_QUEEN_MORGAN.title", DescriptionLocKey = "FGO_SETTINGS_UI_ENABLE_QUEEN_MORGAN.hover.desc")]
    [ModSettingsBinding(Source = ModSettingsReflectionBindingSource.Global)]
    public static bool EnableQueenMorgan { get; set; } = true;
}
