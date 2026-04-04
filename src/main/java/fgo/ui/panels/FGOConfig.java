package fgo.ui.panels;

import static fgo.FGOMod.imagePath;
import static fgo.FGOMod.makeID;

import java.util.Properties;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;

import com.badlogic.gdx.graphics.Texture;
import com.evacipated.cardcrawl.modthespire.lib.SpireConfig;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.localization.UIStrings;

import basemod.BaseMod;
import basemod.ModLabel;
import basemod.ModLabeledToggleButton;
import basemod.ModMinMaxSlider;
import basemod.ModPanel;
import fgo.utils.TextureLoader;

public class FGOConfig {
    private static final Logger logger = LogManager.getLogger(FGOConfig.class.getName());
    public static final String BADGE_IMAGE = imagePath("badge");
    
    public static final String BASE_NP_PER_COST = "baseNPPerCost";
    public static final String ENABLE_COLORLESS_CARDS = "enableColorlessCards";
    public static final String ENABLE_PADORU = "enablePadoru";
    public static final String ENABLE_FTUE = "enableFtue";
    public static final String ENABLE_NO_COST_NOBLE_PHANTASM = "enableNoCostNoblePhantasm";
    public static final String ENABLE_EMIYA = "enableEmiya";
    public static final String ENABLE_CALAMITY_OF_NORWICH = "enableCalamityofNorwich";
    public static final String ENABLE_CERNUNNOS = "enableCernunnos";
    public static final String ENABLE_FAERIE_KNIGHT_GAWAIN = "enableFaerieKnightGawain";
    public static final String ENABLE_FAERIE_KNIGHT_LANCELOT = "enableFaerieKnightLancelot";
    public static final String ENABLE_MOSS = "enableMoss";
    public static final String ENABLE_QUEEN_MORGAN = "enableQueenMorgan";
    
    public static SpireConfig config = null;
    public static Properties defaultSettings = new Properties();
    
    public static int baseNPPerCost = 3;
    public static boolean enableColorlessCards = true;
    public static boolean enablePadoru = true;
    public static boolean enableFtue = true;
    public static boolean enableNoCostNoblePhantasm = true;
    public static boolean enableEmiya = true;
    public static boolean enableCalamityofNorwich = true;
    public static boolean enableCernunnos = true;
    public static boolean enableFaerieKnightGawain = true;
    public static boolean enableFaerieKnightLancelot = true;
    public static boolean enableMoss = true;
    public static boolean enableQueenMorgan = true;
    
    public static void initModSettings() {
        defaultSettings.setProperty(BASE_NP_PER_COST, "3");
        defaultSettings.setProperty(ENABLE_COLORLESS_CARDS, "TRUE");
        defaultSettings.setProperty(ENABLE_PADORU, "TRUE");
        defaultSettings.setProperty(ENABLE_FTUE, "TRUE");
        defaultSettings.setProperty(ENABLE_NO_COST_NOBLE_PHANTASM, "TRUE");
        defaultSettings.setProperty(ENABLE_EMIYA, "TRUE");
        defaultSettings.setProperty(ENABLE_CALAMITY_OF_NORWICH, "TRUE");
        defaultSettings.setProperty(ENABLE_CERNUNNOS, "TRUE");
        defaultSettings.setProperty(ENABLE_FAERIE_KNIGHT_GAWAIN, "TRUE");
        defaultSettings.setProperty(ENABLE_FAERIE_KNIGHT_LANCELOT, "TRUE");
        defaultSettings.setProperty(ENABLE_MOSS, "TRUE");
        defaultSettings.setProperty(ENABLE_QUEEN_MORGAN, "TRUE");
        
        try {
            config = new SpireConfig("fgoMod", "fgoConfig", defaultSettings);
            config.load();
            
            baseNPPerCost = config.getInt(BASE_NP_PER_COST);
            enableColorlessCards = config.getBool(ENABLE_COLORLESS_CARDS);
            enablePadoru = config.getBool(ENABLE_PADORU);
            enableFtue = config.getBool(ENABLE_FTUE);
            enableNoCostNoblePhantasm = config.getBool(ENABLE_NO_COST_NOBLE_PHANTASM);
            enableEmiya = config.getBool(ENABLE_EMIYA);
            enableCalamityofNorwich = config.getBool(ENABLE_CALAMITY_OF_NORWICH);
            enableCernunnos = config.getBool(ENABLE_CERNUNNOS);
            enableFaerieKnightGawain = config.getBool(ENABLE_FAERIE_KNIGHT_GAWAIN);
            enableFaerieKnightLancelot = config.getBool(ENABLE_FAERIE_KNIGHT_LANCELOT);
            enableMoss = config.getBool(ENABLE_MOSS);
            enableQueenMorgan = config.getBool(ENABLE_QUEEN_MORGAN);
        } catch (Exception e) {
            logger.error("Failed to load mod settings", e);
        }
    }
    
    public static void initModConfigMenu() {
        Texture badgeTexture = TextureLoader.getTexture(BADGE_IMAGE);
        
        ModPanel settingsPanel = new ModPanel();
        float startingXPos = 360.0f;
        float settingXPos = startingXPos;
        float xSpacing = 280.0f;
        float settingYPos = 750.0f;
        float lineSpacing = 50.0f;
        int columnCount = 0;
        int maxColumns = 4;
        UIStrings uiStrings = CardCrawlGame.languagePack.getUIString(makeID("FGOConfig"));
        
        // Base NP perCost setting (integer)
        // Use separate label and slider like buildSlider pattern
        String baseNPPerCostLabel = uiStrings.TEXT_DICT.get("baseNPPerCost");
        ModLabel baseNPPerCostLabelElement = new ModLabel(baseNPPerCostLabel,
                settingXPos + 15.0f, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                settingsPanel,
                (label) -> {});
        settingsPanel.addUIElement(baseNPPerCostLabelElement);
        
        float textHeight = FontHelper.getHeight(FontHelper.cardDescFont_N, baseNPPerCostLabel, 1f / Settings.scale);
        float textWidth = FontHelper.getWidth(FontHelper.cardDescFont_N, baseNPPerCostLabel, 1f / Settings.scale);
        float sliderXPos = Math.max(settingXPos + 200.0f, settingXPos + 50.0f + textWidth);

        ModMinMaxSlider baseNPPerCostSlider = new ModMinMaxSlider("",
                sliderXPos, settingYPos + textHeight / 2.0f, 1.0f, 5.0f, (float) baseNPPerCost, "%.0f", settingsPanel,
                (slider) -> {
                    baseNPPerCost = (int)slider.getValue();
                    try {
                        config.setInt(BASE_NP_PER_COST, baseNPPerCost);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save baseNPPerCost setting", e);
                    }
                });
        settingsPanel.addUIElement(baseNPPerCostSlider);
        
        settingYPos -= lineSpacing * 1.5;
        
        // Enable Colorless Cards
        ModLabeledToggleButton enableColorlessCardsButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableColorlessCards"),
                settingXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableColorlessCards,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableColorlessCards = button.enabled;
                    try {
                        config.setBool(ENABLE_COLORLESS_CARDS, enableColorlessCards);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableColorlessCards setting", e);
                    }
                });
        settingsPanel.addUIElement(enableColorlessCardsButton);
        
        settingYPos -= lineSpacing;
        
        // Enable Padoru
        ModLabeledToggleButton enablePadoruButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enablePadoru"),
                settingXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enablePadoru,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enablePadoru = button.enabled;
                    try {
                        config.setBool(ENABLE_PADORU, enablePadoru);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enablePadoru setting", e);
                    }
                });
        settingsPanel.addUIElement(enablePadoruButton);
        
        settingYPos -= lineSpacing;
        
        // Enable FTUE
        ModLabeledToggleButton enableFtueButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableFtue"),
                settingXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableFtue,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableFtue = button.enabled;
                    try {
                        config.setBool(ENABLE_FTUE, enableFtue);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableFtue setting", e);
                    }
                });
        settingsPanel.addUIElement(enableFtueButton);
        
        settingYPos -= lineSpacing;
        
        // Enable No Cost Noble Phantasm
        ModLabeledToggleButton enableNoCostNoblePhantasmButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableNoCostNoblePhantasm"),
                settingXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableNoCostNoblePhantasm,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableNoCostNoblePhantasm = button.enabled;
                    try {
                        config.setBool(ENABLE_NO_COST_NOBLE_PHANTASM, enableNoCostNoblePhantasm);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableNoCostNoblePhantasm setting", e);
                    }
                });
        settingsPanel.addUIElement(enableNoCostNoblePhantasmButton);
        
        settingYPos -= lineSpacing * 1.5;
        
        // Enable Enemies main checkbox with select all functionality
        ModLabeledToggleButton enableEnemiesButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableEnemies"),
                settingXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableEmiya && enableCalamityofNorwich && enableCernunnos && enableFaerieKnightGawain && enableFaerieKnightLancelot && enableMoss && enableQueenMorgan,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    boolean enabled = button.enabled;
                    enableEmiya = enabled;
                    enableCalamityofNorwich = enabled;
                    enableCernunnos = enabled;
                    enableFaerieKnightGawain = enabled;
                    enableFaerieKnightLancelot = enabled;
                    enableMoss = enabled;
                    enableQueenMorgan = enabled;
                    try {
                        config.setBool(ENABLE_EMIYA, enabled);
                        config.setBool(ENABLE_CALAMITY_OF_NORWICH, enabled);
                        config.setBool(ENABLE_CERNUNNOS, enabled);
                        config.setBool(ENABLE_FAERIE_KNIGHT_GAWAIN, enabled);
                        config.setBool(ENABLE_FAERIE_KNIGHT_LANCELOT, enabled);
                        config.setBool(ENABLE_MOSS, enabled);
                        config.setBool(ENABLE_QUEEN_MORGAN, enabled);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableEnemies settings", e);
                    }
                });
        settingsPanel.addUIElement(enableEnemiesButton);
        
        settingYPos -= lineSpacing;
        
        // Monster enable settings - 4 column layout
        columnCount = 0;
        maxColumns = 4;
        float currentXPos = startingXPos;
        
        // Enable Emiya
        ModLabeledToggleButton enableEmiyaButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableEmiya"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableEmiya,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableEmiya = button.enabled;
                    try {
                        config.setBool(ENABLE_EMIYA, enableEmiya);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableEmiya setting", e);
                    }
                });
        settingsPanel.addUIElement(enableEmiyaButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }
        
        // Enable Calamity of Norwich
        ModLabeledToggleButton enableCalamityofNorwichButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableCalamityofNorwich"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableCalamityofNorwich,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableCalamityofNorwich = button.enabled;
                    try {
                        config.setBool(ENABLE_CALAMITY_OF_NORWICH, enableCalamityofNorwich);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableCalamityofNorwich setting", e);
                    }
                });
        settingsPanel.addUIElement(enableCalamityofNorwichButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }
        
        // Enable Cernunnos
        ModLabeledToggleButton enableCernunnosButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableCernunnos"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableCernunnos,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableCernunnos = button.enabled;
                    try {
                        config.setBool(ENABLE_CERNUNNOS, enableCernunnos);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableCernunnos setting", e);
                    }
                });
        settingsPanel.addUIElement(enableCernunnosButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }
        
        // Enable Faerie Knight Lancelot
        ModLabeledToggleButton enableFaerieKnightLancelotButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableFaerieKnightLancelot"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableFaerieKnightLancelot,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableFaerieKnightLancelot = button.enabled;
                    try {
                        config.setBool(ENABLE_FAERIE_KNIGHT_LANCELOT, enableFaerieKnightLancelot);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableFaerieKnightLancelot setting", e);
                    }
                });
        settingsPanel.addUIElement(enableFaerieKnightLancelotButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }

        // Enable Faerie Knight Gawain
        ModLabeledToggleButton enableFaerieKnightGawainButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableFaerieKnightGawain"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableFaerieKnightGawain,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableFaerieKnightGawain = button.enabled;
                    try {
                        config.setBool(ENABLE_FAERIE_KNIGHT_GAWAIN, enableFaerieKnightGawain);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableFaerieKnightGawain setting", e);
                    }
                });
        settingsPanel.addUIElement(enableFaerieKnightGawainButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }
        
        // Enable Moss
        ModLabeledToggleButton enableMossButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableMoss"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableMoss,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableMoss = button.enabled;
                    try {
                        config.setBool(ENABLE_MOSS, enableMoss);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableMoss setting", e);
                    }
                });
        settingsPanel.addUIElement(enableMossButton);
        
        columnCount++;
        if (columnCount >= maxColumns) {
            columnCount = 0;
            currentXPos = startingXPos;
            settingYPos -= lineSpacing;
        } else {
            currentXPos += xSpacing;
        }
        
        // Enable Queen Morgan
        ModLabeledToggleButton enableQueenMorganButton = new ModLabeledToggleButton(uiStrings.TEXT_DICT.get("enableQueenMorgan"),
                currentXPos, settingYPos, Settings.CREAM_COLOR, FontHelper.cardDescFont_N,
                enableQueenMorgan,
                settingsPanel,
                (label) -> {},
                (button) -> {
                    enableQueenMorgan = button.enabled;
                    try {
                        config.setBool(ENABLE_QUEEN_MORGAN, enableQueenMorgan);
                        config.save();
                    } catch (Exception e) {
                        logger.error("Failed to save enableQueenMorgan setting", e);
                    }
                });
        settingsPanel.addUIElement(enableQueenMorganButton);
        
        BaseMod.registerModBadge(badgeTexture, fgo.FGOMod.info.Name, fgo.utils.GeneralUtils.arrToString(fgo.FGOMod.info.Authors), fgo.FGOMod.info.Description, settingsPanel);
    }
}