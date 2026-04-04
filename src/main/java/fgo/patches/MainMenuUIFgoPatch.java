package fgo.patches;

import static fgo.FGOMod.characterPath;
import static fgo.characters.CustomEnums.FGO_MASTER;
import static fgo.ui.panels.MasterSkin.modifierIndexes;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePostfixPatch;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.helpers.Hitbox;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.PowerTip;
import com.megacrit.cardcrawl.helpers.TipHelper;
import com.megacrit.cardcrawl.helpers.controller.CInputActionSet;
import com.megacrit.cardcrawl.helpers.input.InputHelper;
import com.megacrit.cardcrawl.localization.UIStrings;
import com.megacrit.cardcrawl.screens.CharSelectInfo;
import com.megacrit.cardcrawl.screens.charSelect.CharacterOption;
import com.megacrit.cardcrawl.screens.options.DropdownMenu;

import basemod.ReflectionHacks;
import basemod.interfaces.ISubscriber;

public class MainMenuUIFgoPatch implements ISubscriber {
    public static final ArrayList<DropdownMenu> dropdowns = new ArrayList<>();
    public static final ArrayList<String> packSetups = new ArrayList<>();
    private static final Hitbox packDraftToggle = new Hitbox(40.0f * Settings.scale, 40.0f * Settings.scale);
    private static final UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("fgo:PackMainMenuUI");
    private static final String[] TEXT = uiStrings.TEXT;
    private static final ArrayList<PowerTip> toggleTips = new ArrayList<>();
    private static final ArrayList<String> options = new ArrayList<>();
    private static final String[] optionIDs;
    private static final float CHECKBOX_X_OFF = 32.0f * Settings.xScale;
    private static final float CHECKBOX_X;
    private static final float CHECKBOX_Y = Settings.HEIGHT / 2.0f - 175.0f * Settings.yScale;
    private static final float DROPDOWNS_SPACING = 50F * Settings.scale;
    private static final float DROPDOWN_X;
    private static final float DROPDOWNS_START_Y = CHECKBOX_Y + DROPDOWNS_SPACING * (1.5f);
    public static HashMap<String, Integer> idToIndex;
    public static boolean customDraft;

    static {
        options.addAll(Arrays.asList(TEXT).subList(2, 22));

        int optionCount = options.size();
        optionIDs = new String[optionCount];
        idToIndex = new HashMap<>();
        for (int i = 0; i < optionCount; i++) {
            optionIDs[i] = "None" + i;
            idToIndex.put(optionIDs[i], i);
        }

        packSetups.add("None0");

        DropdownMenu d = new DropdownMenu((dropdownMenu, optionIndex, s) -> {
            packSetups.set(0, optionIDs[optionIndex]);
            int autoOptions = idToIndex.size();
            if (optionIndex >= autoOptions) {
                for (DropdownMenu other : dropdowns) {
                    if (other != dropdownMenu && other.getSelectedIndex() == optionIndex) {
                        other.setSelectedIndex(0);
                    }
                }
            }
            modifierIndexes = optionIndex;
        }, options, FontHelper.tipBodyFont, Settings.CREAM_COLOR, 10);
        dropdowns.add(d);

        for (int i = 0; i < dropdowns.size(); i++) {
            DropdownMenu ddm = dropdowns.get(i);
            // 优先使用已保存的 modifierIndexes 作为初始选择，避免覆盖从存档加载的值
            int initial = 0;
            try {
                initial = Math.max(0, Math.min(options.size() - 1, modifierIndexes));
            } catch (Exception e) {
                initial = idToIndex.get(packSetups.get(i));
            }
            ddm.setSelectedIndex(initial);
        }

        float dropdownX = Settings.WIDTH - (50.0f * Settings.scale) - dropdowns.get(0).approximateOverallWidth();
        float checkboxX = Settings.WIDTH - 82F * Settings.scale - FontHelper.getWidth(FontHelper.tipHeaderFont, uiStrings.TEXT[0], 1);

        if (checkboxX - dropdownX >= 60) {
            DROPDOWN_X = dropdownX;
            CHECKBOX_X = checkboxX + CHECKBOX_X_OFF;
        } else {
            DROPDOWN_X = Math.min(dropdownX, checkboxX);
            CHECKBOX_X = DROPDOWN_X + CHECKBOX_X_OFF;
        }

    }

    public static String refreshSkinFgo() {
        String skinPath = characterPath("Master" + modifierIndexes + "");
        if (modifierIndexes == 19) {
            skinPath = characterPath("Cat_Arcueid_Brunestud");
        }
        return skinPath;
    }

    @SpirePatch(clz = CharacterOption.class, method = "renderRelics")
    public static class RenderOptions {
        @SpirePostfixPatch
        public static void Postfix(CharacterOption obj, SpriteBatch sb) {
            CharSelectInfo c = ReflectionHacks.getPrivate(obj, CharacterOption.class, "charInfo");
            if (c != null && c.player.chosenClass.equals(FGO_MASTER) && obj.selected) {
                packDraftToggle.move(CHECKBOX_X, CHECKBOX_Y);
                packDraftToggle.render(sb);

                sb.setColor(Color.WHITE);
                float checkScale = Settings.scale * 0.8f;
                sb.draw(ImageMaster.CHECKBOX, packDraftToggle.cX - 32f, packDraftToggle.cY - 32f, 32.0f, 32.0f, 64.0f, 64.0f, checkScale, checkScale, 0.0f, 0, 0, 64, 64, false, false);
                if (customDraft) {
                    sb.draw(ImageMaster.TICK, packDraftToggle.cX - 32f, packDraftToggle.cY - 32f, 32.0f, 32.0f, 64.0f, 64.0f, checkScale, checkScale, 0.0f, 0, 0, 64, 64, false, false);
                    sb.draw(ImageMaster.loadImage(refreshSkinFgo()), CHECKBOX_X - 25.0f * Settings.xScale - 540F * Settings.scale, Settings.HEIGHT / 2.0f + 75.0f * Settings.yScale - 320F * Settings.scale, 32.0f, 32.0f, 320.0f, 320.0f, Settings.scale, Settings.scale, 0.0f, 0, 0, 320, 320, false, false);
                }
                FontHelper.renderSmartText(sb, FontHelper.tipHeaderFont, uiStrings.TEXT[0], packDraftToggle.cX + 25f * Settings.scale, packDraftToggle.cY + FontHelper.getHeight(FontHelper.tipHeaderFont) * 0.5f, Settings.BLUE_TEXT_COLOR);
                if (customDraft) {
                    for (int i = dropdowns.size() - 1; i >= 0; i--) {
                        dropdowns.get(i).render(sb, DROPDOWN_X, DROPDOWNS_START_Y - (DROPDOWNS_SPACING * i));
                    }
                }
            }
        }
    }

    @SpirePatch(clz = CharacterOption.class, method = "updateHitbox")
    public static class UpdateOptions {
        @SpirePostfixPatch
        public static void Postfix(CharacterOption obj) {
            CharSelectInfo c = ReflectionHacks.getPrivate(obj, CharacterOption.class, "charInfo");

            if (c != null && c.player.chosenClass.equals(FGO_MASTER) && obj.selected) {
                boolean stopInput = false;
                if (customDraft) {
                    for (DropdownMenu d : dropdowns) {
                        if (d.isOpen)
                            stopInput = true;
                        d.update();
                        if (d.isOpen || stopInput) {
                            stopInput = true;
                            InputHelper.justClickedLeft = false;
                            InputHelper.justReleasedClickLeft = false;
                            CInputActionSet.select.unpress();
                            CInputActionSet.proceed.unpress();
                        }
                    }
                }

                if (stopInput)
                    return;

                packDraftToggle.update();
                if (packDraftToggle.hovered) {
                    if (toggleTips.isEmpty()) {
                        toggleTips.add(new PowerTip(uiStrings.TEXT[0], uiStrings.TEXT[1]));
                    }
                    if (InputHelper.mX < 1400.0f * Settings.scale) {
                        TipHelper.queuePowerTips(InputHelper.mX + 60.0f * Settings.scale, InputHelper.mY - 50.0f * Settings.scale, toggleTips);
                    } else {
                        TipHelper.queuePowerTips(InputHelper.mX - 350.0f * Settings.scale, InputHelper.mY - 50.0f * Settings.scale, toggleTips);
                    }

                    if (InputHelper.justClickedLeft) {
                        CardCrawlGame.sound.playA("UI_CLICK_1", -0.4f);
                        packDraftToggle.clickStarted = true;
                    }
                    if (packDraftToggle.clicked) {
                        customDraft = !customDraft;
                        packDraftToggle.clicked = false;
                    }
                }
            }
        }
    }
}
