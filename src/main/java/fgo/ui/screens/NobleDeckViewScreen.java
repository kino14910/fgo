package fgo.ui.screens;

import static fgo.FGOMod.makeID;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.badlogic.gdx.utils.Logger;
import com.evacipated.cardcrawl.modthespire.lib.SpireEnum;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.helpers.MathHelper;
import com.megacrit.cardcrawl.helpers.input.InputHelper;
import com.megacrit.cardcrawl.localization.TutorialStrings;
import com.megacrit.cardcrawl.screens.MasterDeckViewScreen;
import com.megacrit.cardcrawl.screens.mainMenu.ScrollBar;
import com.megacrit.cardcrawl.screens.mainMenu.ScrollBarListener;

import basemod.abstracts.CustomScreen;
import fgo.cards.AbsNoblePhantasmCard;
import fgo.utils.NobleCardGroup;

public class NobleDeckViewScreen extends CustomScreen implements ScrollBarListener {
    private static final TutorialStrings tutorialStrings = CardCrawlGame.languagePack.getTutorialString(makeID(NobleDeckViewScreen.class.getSimpleName()));
    public static final String[] TEXT = NobleDeckViewScreen.tutorialStrings.TEXT;
    private static final String HEADER_INFO;
    
    public static class Enum {
        @SpireEnum
        public static AbstractDungeon.CurrentScreen Noble_Phantasm;
    }

    static {
        drawStartY = (float) Settings.HEIGHT * 0.66f;
        SCROLL_BAR_THRESHOLD = 500.0f * Settings.scale;
        HEADER_INFO = TEXT[0];
    }

    @Override
    public AbstractDungeon.CurrentScreen curScreen() {
        return Enum.Noble_Phantasm;
    }

    NobleCardGroup<AbsNoblePhantasmCard> nobleCards;
    private static float drawStartX;
    private static float drawStartY;
    private static float padX;
    private static float padY;
    private static final float SCROLL_BAR_THRESHOLD;
    private static final int CARDS_PER_LINE = 5;

    private AbstractCard hoveredCard = null;
    private AbstractCard clickStartedCard = null;
    private boolean grabbedScreen = false;
    private float grabStartY = 0.0f;
    private float currentDiffY = 0.0f;
    private float scrollLowerBound = -Settings.DEFAULT_SCROLL_LIMIT;
    private float scrollUpperBound = Settings.DEFAULT_SCROLL_LIMIT;

    private ScrollBar scrollBar;

    Logger logger = new Logger(NobleDeckViewScreen.class.getName());

    public NobleDeckViewScreen() {
        drawStartX = Settings.WIDTH;
        drawStartX -= 5.0f * AbstractCard.IMG_WIDTH * 0.75f;
        drawStartX -= 4.0f * Settings.CARD_VIEW_PAD_X;
        drawStartX /= 2.0f;
        drawStartX += AbstractCard.IMG_WIDTH * 0.75f / 2.0f;
        
        drawStartY = (float) Settings.HEIGHT * 0.66f;
        
        padX = AbstractCard.IMG_WIDTH * 0.75f + Settings.CARD_VIEW_PAD_X;
        padY = AbstractCard.IMG_HEIGHT * 0.75f + Settings.CARD_VIEW_PAD_Y;

        scrollBar = new ScrollBar(this);
        scrollBar.move(0.0f, -30.0f * Settings.scale);
    }

    @Override
    public void open(Object... args) {
        reopen();
        // 保存传入的卡组
        if (args.length > 0 && args[0] instanceof NobleCardGroup) {
            @SuppressWarnings("unchecked")
            NobleCardGroup<AbsNoblePhantasmCard> nobleCards = (NobleCardGroup<AbsNoblePhantasmCard>) args[0];
            this.nobleCards = nobleCards;
            }

        AbstractDungeon.player.releaseCard();
        CardCrawlGame.sound.play("DECK_OPEN");
        
        // 重置滚动位置
        currentDiffY = scrollLowerBound;
        grabStartY = scrollLowerBound;
        grabbedScreen = false;
        
        hideCards();
        
        // 设置屏幕状态
        AbstractDungeon.isScreenUp = true;
        AbstractDungeon.screen = curScreen();

        // 隐藏战斗面板和显示黑屏
        AbstractDungeon.overlayMenu.proceedButton.hide();
        AbstractDungeon.overlayMenu.hideCombatPanels();
        AbstractDungeon.overlayMenu.showBlackScreen();
        AbstractDungeon.overlayMenu.cancelButton.show(TEXT[1]);

        calculateScrollBounds();
    }

    @Override
    public void reopen() {
        Settings.hideRelics = true;
        AbstractDungeon.isScreenUp = true;
        AbstractDungeon.screen = curScreen();
        AbstractDungeon.overlayMenu.showBlackScreen();
        AbstractDungeon.dynamicBanner.hide(); // Hide banners that get in the way
        AbstractDungeon.dungeonMapScreen.map.hideInstantly(); // Because the map won't be hidden properly otherwise
        AbstractDungeon.gridSelectScreen.hide(); // Because this has to be called at least once to prevent softlocks when upgrading cards
        AbstractDungeon.overlayMenu.cancelButton.show(MasterDeckViewScreen.TEXT[1]);
    }

    @Override
    public void close() {
        switchScreen();
    }

    @Override
    public void openingDeck() {
        switchScreen();
    }

    @Override
    public void openingMap() {
        switchScreen();
    }

    @Override
    public void openingSettings() {
        switchScreen();
    }

    @Override
    public void scrolledUsingBar(float newPercent) {
        currentDiffY = MathHelper.valueFromPercentBetween(scrollLowerBound, scrollUpperBound, newPercent);
        updateBarPosition();
    }

    @Override
    public void render(SpriteBatch sb) {
        nobleCards.renderTip(sb);
        FontHelper.renderDeckViewTip(sb, HEADER_INFO, 96.0f * Settings.scale, Settings.CREAM_COLOR);

        if (shouldShowScrollBar()) {
            scrollBar.render(sb);
        }

        if (nobleCards == null) {
            return;
        }

        if (hoveredCard == null) {
            nobleCards.render(sb);
            return;
        } 

        nobleCards.group.forEach(card -> {
            if (card != hoveredCard) {
                card.render(sb);
            }
        });
        
        hoveredCard.renderHoverShadow(sb);
        hoveredCard.render(sb);
    }

    @Override
    public void update() {
        boolean isDraggingScrollBar = false;
        if (shouldShowScrollBar()) {
            isDraggingScrollBar = scrollBar.update();
        }
        
        if (!isDraggingScrollBar) {
            updateScrolling();
        }
        
        updatePositions();
        updateClicking();
    }

    @Override
    public boolean allowOpenDeck() {
        return true;
    }

    @Override
    public boolean allowOpenMap() {
        return true;
    }

    private void calculateScrollBounds() {
        if (nobleCards == null || nobleCards.size() <= 10) {
            scrollUpperBound = Settings.DEFAULT_SCROLL_LIMIT;
            return;
        }
        
        int scrollTmp = nobleCards.size() / CARDS_PER_LINE - 2;
        if (nobleCards.size() % CARDS_PER_LINE != 0) {
            ++scrollTmp;
        }
        scrollUpperBound = Settings.DEFAULT_SCROLL_LIMIT + (float) scrollTmp * padY;    

    }

    private void hideCards() {
        if (nobleCards == null) return;
        
        int lineNum = 0;
        for (int i = 0; i < nobleCards.size(); ++i) {
            int mod = i % CARDS_PER_LINE;
            if (mod == 0 && i != 0) {
                ++lineNum;
            }
            AbstractCard card = nobleCards.group.get(i);
            card.current_x = drawStartX + (float) mod * padX;
            card.current_y = drawStartY + currentDiffY - (float) lineNum * padY - MathUtils.random(100.0f * Settings.scale, 200.0f * Settings.scale);
            card.targetDrawScale = 0.75f;
            card.drawScale = 0.75f;
            card.setAngle(0.0f, true);
        }
    }

    private void updateScrolling() {
        int y = InputHelper.mY;
        if (!grabbedScreen) {
            if (InputHelper.scrolledDown) {
                currentDiffY += Settings.SCROLL_SPEED;
            } else if (InputHelper.scrolledUp) {
                currentDiffY -= Settings.SCROLL_SPEED;
            }
            if (InputHelper.justClickedLeft) {
                grabbedScreen = true;
                grabStartY = (float) y - currentDiffY;
            }
        } else if (InputHelper.isMouseDown) {
            currentDiffY = (float) y - grabStartY;
        } else {
            grabbedScreen = false;
        }
        
        resetScrolling();
        updateBarPosition();
    }

    private void updatePositions() {
        hoveredCard = null;
        if (nobleCards == null) return;
        
        int lineNum = 0;
        for (int i = 0; i < nobleCards.size(); ++i) {
            int mod = i % CARDS_PER_LINE;
            if (mod == 0 && i != 0) {
                ++lineNum;
            }
            
            AbstractCard card = nobleCards.group.get(i);
            card.target_x = drawStartX + (float) mod * padX;
            card.target_y = drawStartY + currentDiffY - (float) lineNum * padY;
            card.update();
            card.updateHoverLogic();
            
            if (card.hb.hovered) {
                hoveredCard = card;
            }
        }
    }

    private void updateClicking() {
        if (hoveredCard == null) {
            clickStartedCard = null;
            return;
        }
        if (InputHelper.justClickedLeft) {
            clickStartedCard = hoveredCard;
        }
        
        if ((InputHelper.justReleasedClickLeft && hoveredCard == clickStartedCard)) {
            InputHelper.justReleasedClickLeft = false;
            CardCrawlGame.cardPopup.open(hoveredCard, nobleCards);
            clickStartedCard = null;
        }
    }

    private void resetScrolling() {
        if (currentDiffY < scrollLowerBound) {
            currentDiffY = MathHelper.scrollSnapLerpSpeed(currentDiffY, scrollLowerBound);
        } else if (currentDiffY > scrollUpperBound) {
            currentDiffY = MathHelper.scrollSnapLerpSpeed(currentDiffY, scrollUpperBound);
        }
    }

    private boolean shouldShowScrollBar() {
        return scrollUpperBound > SCROLL_BAR_THRESHOLD;
    }

    private void updateBarPosition() {
        float percent = MathHelper.percentFromValueBetween(scrollLowerBound, scrollUpperBound, currentDiffY);
        scrollBar.parentScrolledToPercent(percent);
    }

    public void switchScreen() {
        Settings.hideRelics = false;
        genericScreenOverlayReset();
        AbstractDungeon.overlayMenu.cancelButton.hide();
        if (AbstractDungeon.previousScreen != AbstractDungeon.CurrentScreen.CARD_REWARD) {
            AbstractDungeon.overlayMenu.hideBlackScreen();
        }
        AbstractDungeon.isScreenUp = false;
    }
}