package fgo.patches;

import java.util.ArrayList;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Vector2;
import com.evacipated.cardcrawl.modthespire.lib.ByRef;
import com.evacipated.cardcrawl.modthespire.lib.LineFinder;
import com.evacipated.cardcrawl.modthespire.lib.Matcher;
import com.evacipated.cardcrawl.modthespire.lib.SpireInsertLocator;
import com.evacipated.cardcrawl.modthespire.lib.SpireInsertPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePrefixPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpireReturn;
import com.evacipated.cardcrawl.modthespire.patcher.PatchingException;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.screens.SingleCardViewPopup;

import basemod.ReflectionHacks;
import basemod.patches.com.megacrit.cardcrawl.cards.AbstractCard.MultiCardPreview;
import basemod.patches.com.megacrit.cardcrawl.cards.AbstractCard.MultiCardPreviewRenderer;
import fgo.hexui_lib.CardRenderer;
import fgo.hexui_lib.interfaces.CustomCardPortrait;
import fgo.hexui_lib.interfaces.CustomCardTypeLocation;
import fgo.hexui_lib.util.FloatPair;
import javassist.CannotCompileException;
import javassist.CtBehavior;

@SuppressWarnings({"rawtypes", "unused"})
public class RenderCardPatch {
    @SpirePatch(clz = AbstractCard.class, method = "getCardBg")
    public static class GetCardBgPatch {
        @SpireInsertPatch(
                rloc = 0
        )
        public static SpireReturn<Texture> Insert(AbstractCard card) {
            if (card instanceof CustomCardPortrait) {
                CustomCardPortrait customPortraitCard = (CustomCardPortrait) card;
            }
            return SpireReturn.Continue();
        }
    }

    @SpirePatch(clz = AbstractCard.class, method = "renderPortraitFrame")
    public static class RenderCardPortraitFramePatch {
        @SpireInsertPatch(
                rloc = 0,
                localvars = {"renderColor"}
        )
        public static SpireReturn Insert(AbstractCard card, final SpriteBatch sb, float x, float y, Color renderColor) {
            if (card instanceof CustomCardPortrait) {
                return SpireReturn.Return(null);
            }
            return SpireReturn.Continue();
        }
    }

    @SpirePatch(clz = AbstractCard.class, method = "renderPortrait")
    public static class RenderCardPortraitPatch {
        @SpireInsertPatch(
                rloc = 0,
                localvars = {"renderColor"}
        )
        public static SpireReturn Insert(AbstractCard card, SpriteBatch sb, Color renderColor) {
            if (card instanceof CustomCardPortrait) {
                sb.setColor(renderColor);
                float drawX = card.current_x - 256.0f;
                float drawY = card.current_y - 256.0f;

                CardRenderer.renderPortrait(sb, card, drawX, drawY, false);
                return SpireReturn.Return(null);
            }
            return SpireReturn.Continue();
        }
    }

    @SpirePatch(clz = AbstractCard.class, method = "renderType")
    public static class RenderCardTypePatch {
        @SpireInsertPatch(
                locator = Locator.class,
                localvars = {"font", "text", "renderColor"}
        )
        public static SpireReturn Insert(AbstractCard card, SpriteBatch sb, BitmapFont font, String text, Color renderColor) {
            if (card instanceof CustomCardTypeLocation) {
                CustomCardTypeLocation locationCard = (CustomCardTypeLocation) card;
                FloatPair location = locationCard.getCardTypeLocation(new FloatPair(0f, -22f), false);

                FontHelper.renderRotatedText(sb, font, text,
                        card.current_x,
                        card.current_y - (location.y - 7.0f) * card.drawScale * Settings.scale,
                        0.0f, 
                        -1.0f * card.drawScale * Settings.scale,
                        card.angle, false,
                        new Color(0.35F, 0.35F, 0.35F, 1.0f));

                return SpireReturn.Return(null);
            }
            return SpireReturn.Continue();
        }

        private static class Locator extends SpireInsertLocator {
            @Override
            public int[] Locate(CtBehavior ctMethodToPatch) throws CannotCompileException, PatchingException {
                Matcher finalMatcher = new Matcher.MethodCallMatcher(
                        FontHelper.class, "renderRotatedText");
                return LineFinder.findInOrder(ctMethodToPatch, new ArrayList<>(), finalMatcher);
            }
        }
    }
    
	@SpirePatch(clz = SingleCardViewPopup.class, method = "renderCardBanner", paramtypez = {SpriteBatch.class})
	public static class RenderCardBannerPatch {
		@SpirePrefixPatch
		public static SpireReturn<Void> Prefix(SingleCardViewPopup _inst, SpriteBatch sb, AbstractCard ___card) {
			if (___card instanceof CustomCardPortrait) {
				return SpireReturn.Return();
			}
			return SpireReturn.Continue();
		}
	}

    @SpirePatch(clz = SingleCardViewPopup.class, method = "renderFrame")
    public static class RenderSingleCardPortraitFramePatch {
        @SpireInsertPatch(
                rloc = 0,
                localvars = {"card"}
        )
        public static SpireReturn Insert(SingleCardViewPopup view, final SpriteBatch sb, AbstractCard card) {
            if (card instanceof CustomCardPortrait) {
                return SpireReturn.Return(null);
            }
            return SpireReturn.Continue();
        }
    }

    @SpirePatch(clz = SingleCardViewPopup.class, method = "renderPortrait")
    public static class RenderSingleCardPortraitPatch {
        @SpireInsertPatch(
                rloc = 0,
                localvars = {"card"}
        )
        public static SpireReturn Insert(SingleCardViewPopup view, final SpriteBatch sb, AbstractCard card) {
            if (card instanceof CustomCardPortrait) {
                float drawX = Settings.WIDTH / 2.0f - 512f;
                float drawY = Settings.HEIGHT / 2.0f - 512f;

                CardRenderer.renderPortrait(sb, card, drawX, drawY, true);
                return SpireReturn.Return(null);

            }
            return SpireReturn.Continue();
        }
    }

    @SpirePatch(clz = SingleCardViewPopup.class, method = "renderCardTypeText", paramtypez = {SpriteBatch.class})
    public static class RenderSingleCardTypePatch {
        @SpireInsertPatch(
                locator = Locator.class,
                localvars = {"label", "CARD_TYPE_COLOR", "card"}
        )
        public static SpireReturn Insert(SingleCardViewPopup view, SpriteBatch sb, String label, Color CARD_TYPE_COLOR, AbstractCard card) {
            if (card instanceof CustomCardTypeLocation) {
                CustomCardTypeLocation locationCard = (CustomCardTypeLocation) card;
                FloatPair location = locationCard.getCardTypeLocation(new FloatPair(3f, -40f), true);

                FontHelper.renderFontCentered(sb, FontHelper.panelNameFont, label,
                        Settings.WIDTH / 2.0f + location.x * Settings.scale, Settings.HEIGHT / 2.0f - location.y * Settings.scale,
                        new Color(0.35F, 0.35F, 0.35F, 1.0f));

                return SpireReturn.Return();
            }
            return SpireReturn.Continue();
        }

        private static class Locator extends SpireInsertLocator {
            @Override
            public int[] Locate(CtBehavior ctBehavior) throws CannotCompileException, PatchingException {
                return LineFinder.findInOrder(ctBehavior,
                            new Matcher.MethodCallMatcher(FontHelper.class, "renderFontCentered"));
            }
        }

	@SpirePatch(clz = SingleCardViewPopup.class, method = "renderTips", paramtypez = {SpriteBatch.class})
	public static class RenderTipsPatch {
		private static class Locator extends SpireInsertLocator {
			@Override
			public int[] Locate(CtBehavior ctBehavior) throws CannotCompileException, PatchingException {
				return LineFinder.findInOrder(ctBehavior,
						new Matcher.MethodCallMatcher(AbstractCard.class,
								"renderCardPreviewInSingleView"));
			}
		}
	}

        @SpirePatch(clz = MultiCardPreviewRenderer.RenderMultiCardPreviewInSingleViewPatch.class,
                method = "Postfix", paramtypez = {SingleCardViewPopup.class, SpriteBatch.class})
        public static class AvoidOverlappingWithPanelPatch {
            @SpireInsertPatch(rloc = 25, localvars = {"horizontalOnly", "verticalNext", "position", "offset", "toPreview"})
            public static void Insert(SingleCardViewPopup _inst, SpriteBatch sb, AbstractCard ___card,
                                    boolean horizontalOnly, @ByRef boolean[] verticalNext,
                                    Vector2 position, Vector2 offset, AbstractCard toPreview) {
                if (___card instanceof CustomCardTypeLocation) {

                    ArrayList<AbstractCard> previews = MultiCardPreview.multiCardPreview.get(___card);

                    if (previews.size() > 1 && toPreview == previews.get(1)) {
                        ReflectionHacks.privateMethod(MultiCardPreviewRenderer.class, "reposition",
                                Vector2.class, Vector2.class, boolean.class)
                                .invoke(null, position, offset, horizontalOnly || !verticalNext[0]);

                        verticalNext[0] = !verticalNext[0];
                    }
                }
            }
        }
    }
}
