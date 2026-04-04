package fgo.patches;

import static fgo.FGOMod.imagePath;
import static fgo.FGOMod.makeID;
import static fgo.characters.CustomEnums.FGO_MASTER;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePostfixPatch;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.helpers.Hitbox;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.controller.CInputActionSet;
import com.megacrit.cardcrawl.helpers.input.InputHelper;
import com.megacrit.cardcrawl.localization.UIStrings;
import com.megacrit.cardcrawl.screens.charSelect.CharacterOption;
import com.megacrit.cardcrawl.screens.charSelect.CharacterSelectScreen;

public class PictureSelectFgoPatch {
    private static final UIStrings uiStrings = CardCrawlGame.languagePack.getUIString(makeID(PictureSelectFgoPatch.class.getSimpleName()));
    public static final String[] TEXT = uiStrings.TEXT;
    public static int TalentCount = 0;
    public static Hitbox TalentRight;
    public static Hitbox TalentLeft;
    private static final float Talent_RIGHT_W = FontHelper.getSmartWidth(FontHelper.cardTitleFont, TEXT[1], 9999.0f, 0.0f);
    
    @SpirePatch(clz = CharacterSelectScreen.class, method = "initialize")
    public static class CharacterSelectScreenPatch_Initialize {
        @SpirePostfixPatch
        public static void Initialize(CharacterSelectScreen __instance) {
            // Called when you first open the screen, create hitbox for each button
            TalentRight = new Hitbox(70.0f * Settings.scale, 70.0f * Settings.scale);
            TalentLeft = new Hitbox(70.0f * Settings.scale, 70.0f * Settings.scale);
            TalentRight.move(Settings.WIDTH / 2.0f + 500.0f * Settings.scale + Talent_RIGHT_W * 1.5f, 70.0f * Settings.scale);
            TalentLeft.move(Settings.WIDTH / 2.0f + 500.0f * Settings.scale - Talent_RIGHT_W * 0.5f, 70.0f * Settings.scale);
        }
    }

    @SpirePatch(clz = CharacterSelectScreen.class, method = "render")
    public static class CharacterSelectScreenPatch_Render {
        @SpirePostfixPatch
        public static void Postfix(CharacterSelectScreen __instance, SpriteBatch sb) {
            // Render your buttons/images by passing SpriteBatch
            if (TalentCount != 0 && TalentCount != 1) {
                TalentCount = 0;
            }

            for (CharacterOption o : __instance.options) {
                if (o.selected && o.c.chosenClass == FGO_MASTER) {
                    TalentRight.render(sb);
                    TalentLeft.render(sb);

                    if (TalentRight.hovered || Settings.isControllerMode) {
                        sb.setColor(Color.WHITE);
                    } else {
                        sb.setColor(Color.LIGHT_GRAY);
                    }

                    sb.draw(ImageMaster.CF_RIGHT_ARROW, TalentRight.cX - 24.0f, TalentRight.cY - 24.0f, 24.0f, 24.0f, 48.0f, 48.0f, Settings.scale, Settings.scale, 0.0f, 0, 0, 48, 48, false, false);

                    if (TalentLeft.hovered || Settings.isControllerMode) {
                        sb.setColor(Color.WHITE);
                    } else {
                        sb.setColor(Color.LIGHT_GRAY);
                    }

                    sb.draw(ImageMaster.CF_LEFT_ARROW, TalentLeft.cX - 24.0f, TalentLeft.cY - 24.0f, 24.0f, 24.0f, 48.0f, 48.0f, Settings.scale, Settings.scale, 0.0f, 0, 0, 48, 48, false, false);

                    FontHelper.renderFontCentered(sb, FontHelper.smallDialogOptionFont, TEXT[TalentCount + 2], Settings.WIDTH / 2.0f + Talent_RIGHT_W / 2.0f + 500.0f * Settings.scale, 35.0f * Settings.scale, Settings.CREAM_COLOR.cpy());
                    FontHelper.renderFontCentered(sb, FontHelper.cardTitleFont, TEXT[TalentCount], Settings.WIDTH / 2.0f + Talent_RIGHT_W / 2.0f + 500.0f * Settings.scale, TalentRight.cY, Settings.GOLD_COLOR.cpy());
                }
            }

        }
    }

    @SpirePatch(clz = CharacterSelectScreen.class, method = "update")
    public static class CharacterSelectScreenPatch_Update {
        @SpirePostfixPatch
        public static void Postfix(CharacterSelectScreen __instance) {
            // Update your buttons position, check if the player clicked them, and do something if they did
            for (CharacterOption o : __instance.options) {
                if (!o.selected || o.c.chosenClass != FGO_MASTER) {
                    continue;
                }

                if (InputHelper.justClickedLeft && TalentLeft.hovered) {
                    TalentLeft.clickStarted = true;
                    CardCrawlGame.sound.play("UI_CLICK_1");
                }

                if (InputHelper.justClickedLeft && TalentRight.hovered) {
                    TalentRight.clickStarted = true;
                    CardCrawlGame.sound.play("UI_CLICK_1");
                }

                if (TalentLeft.justHovered || TalentRight.justHovered) {
                    CardCrawlGame.sound.playV("UI_HOVER", 0.75f);
                }

                if (TalentCount != 0 && TalentCount != 1) {
                    TalentCount = 0;
                }

                if (TalentRight.clicked || CInputActionSet.pageRightViewExhaust.isJustPressed()) {
                    TalentRight.clicked = false;
                    TalentCount = (TalentCount < 1) ? TalentCount + 1 : 0;
                    __instance.bgCharImg = updateBgImg();
                }

                if (TalentLeft.clicked || CInputActionSet.pageLeftViewDeck.isJustPressed()) {
                    TalentLeft.clicked = false;
                    TalentCount = (TalentCount > 0) ? TalentCount - 1 : 1;
                    __instance.bgCharImg = updateBgImg();
                }

                TalentLeft.update();
                TalentRight.update();
            }
        }
    }

    public static Texture updateBgImg() {
        switch (TalentCount) {
            case 0:
                return new Texture(imagePath("charSelect/MasterPortrait"));
            case 1:
                return new Texture(imagePath("charSelect/MasterPortrait1"));
            default:
                return null;
        }
    }
}