package fgo.vfx;

import static fgo.FGOMod.makeID;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.Map;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.Interpolation;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.CardLibrary;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.rooms.AbstractRoom;
import com.megacrit.cardcrawl.rooms.CampfireUI;
import com.megacrit.cardcrawl.rooms.RestRoom;
import com.megacrit.cardcrawl.vfx.AbstractGameEffect;

import fgo.cards.derivative.mash.Camelot;
import fgo.cards.derivative.mash.LordCamelot;
import fgo.cards.derivative.mash.LordChaldeas;
import fgo.cards.derivative.mash.RayProofKyrielight;
import fgo.cards.noblecards.HollowHeartAlbion;
import fgo.cards.noblecards.LastSunXibalba;
import fgo.cards.noblecards.Unlimited;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.relics.ArchetypeORT;
import fgo.ui.campfire.AlterOption;
import fgo.ui.panels.NobleDeckCards;
import fgo.utils.NobleCardHelper;

public class AlterOptionEffect extends AbstractGameEffect {
    private static final String[] TEXT = CardCrawlGame.languagePack.getUIString(makeID(AlterOptionEffect.class.getSimpleName())).TEXT;
    private final Color screenColor;
    private boolean openedScreen = false;
    
    public AlterOptionEffect() {
        screenColor = AbstractDungeon.fadeColor.cpy();
        duration = 1.5f;
        screenColor.a = 0.0f;
        AbstractDungeon.overlayMenu.proceedButton.hide();
    }

    @Override
    public void update() {
        if (!AbstractDungeon.isScreenUp) {
            duration -= Gdx.graphics.getDeltaTime();
            updateBlackScreenColor();
        }

        if (!AbstractDungeon.isScreenUp && !AbstractDungeon.gridSelectScreen.selectedCards.isEmpty()) {
            AbstractCard c = AbstractDungeon.gridSelectScreen.selectedCards.get(0);
            // AbstractDungeon.topLevelEffects.add(new ShowCardAndObtainEffect(c, Settings.WIDTH / 2.0f + 190.0f * Settings.scale, Settings.HEIGHT / 2.0f));
            // NobleDeckPanelItem.addCard(c.makeCopy());
            NobleDeckCards.addCard(c.cardID);
            AlterOption.usedIdentify = true;
            AbstractDungeon.gridSelectScreen.selectedCards.clear();
            ((RestRoom) AbstractDungeon.getCurrRoom()).fadeIn();
        }

        if (duration < 1.0f && !openedScreen) {
            openedScreen = true;

            CardGroup group = new CardGroup(CardGroup.CardGroupType.UNSPECIFIED);
            ArrayList<AbstractCard> noblePhantasmCards = new ArrayList<>();
            for (Map.Entry<String, AbstractCard> stringAbstractCardEntry : CardLibrary.cards.entrySet()) {
                if (stringAbstractCardEntry.getValue().color == FGOCardColor.NOBLE_PHANTASM) {
                    noblePhantasmCards.add(stringAbstractCardEntry.getValue().makeCopy());
                }
            }

            HashSet<String> excludedCards = new HashSet<>(Arrays.asList(
                HollowHeartAlbion.ID, 
                Unlimited.ID,
                Camelot.ID,
                LordCamelot.ID,
                LordChaldeas.ID,
                RayProofKyrielight.ID,
                LastSunXibalba.ID
            ));

            if (AbstractDungeon.player.hasRelic(ArchetypeORT.ID)) {
                excludedCards.remove(LastSunXibalba.ID);
            }

            for (AbstractCard card : noblePhantasmCards) {
                if (!NobleCardHelper.hasCardWithID(card.cardID)
                        && !excludedCards.contains(card.cardID)) {
                    group.addToBottom(card);
                }
            }

            AbstractDungeon.gridSelectScreen.open(group, 1, TEXT[0], false, false, false, false);
        }

        if (duration < 0.0f) {
            isDone = true;
            if (CampfireUI.hidden) {
                AbstractRoom.waitTimer = 0.0f;
                AbstractDungeon.getCurrRoom().phase = AbstractRoom.RoomPhase.INCOMPLETE;
                ((RestRoom) AbstractDungeon.getCurrRoom()).campfireUI.reopen();
                ((RestRoom) AbstractDungeon.getCurrRoom()).cutFireSound();
            }
        }

    }

    private void updateBlackScreenColor() {
        if (duration > 1.0f) {
            screenColor.a = Interpolation.fade.apply(1.0f, 0.0f, (duration - 1.0f) * 2.0f);
        } else {
            screenColor.a = Interpolation.fade.apply(0.0f, 1.0f, duration / 1.5f);
        }

    }

    @Override
    public void render(SpriteBatch sb) {
        sb.setColor(screenColor);
        sb.draw(ImageMaster.WHITE_SQUARE_IMG, 0.0f, 0.0f, (float) Settings.WIDTH, (float) Settings.HEIGHT);
        if (AbstractDungeon.screen == AbstractDungeon.CurrentScreen.GRID) {
            AbstractDungeon.gridSelectScreen.render(sb);
        }

    }

    @Override
    public void dispose() {
    }
}