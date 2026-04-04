package fgo.action;

import static fgo.utils.ModHelper.getPowerCount;

import java.util.ArrayList;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInHandAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.unlock.UnlockTracker;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ForcedNPCardPower;
import fgo.powers.NPOverChargePower;
import fgo.ui.panels.NobleDeckCards;
import fgo.utils.NobleCardGroup;

public class NoblePhantasmSelectAction extends AbstractGameAction {
    private static final String[] NPTEXT = CardCrawlGame.languagePack.getUIString("fgo:NPText").TEXT;
    
    public NoblePhantasmSelectAction() {
        actionType = ActionType.CARD_MANIPULATION;
        duration = Settings.ACTION_DUR_MED;
    }

    @Override
    public void update() {
        AbstractPlayer p = AbstractDungeon.player;
        if (duration == Settings.ACTION_DUR_MED) {
            int OCAmt = getPowerCount(p, NPOverChargePower.POWER_ID);
            if (p.hasPower(ForcedNPCardPower.POWER_ID)) {
                ForcedNPCardPower npCardPower = (ForcedNPCardPower) p.getPower(ForcedNPCardPower.POWER_ID);
                AbsNoblePhantasmCard card = (AbsNoblePhantasmCard) npCardPower.card;
                UnlockTracker.markCardAsSeen(card.cardID);
                for (int i = 0; i < OCAmt; i++) {
                    card.upgrade();
                }
                addToBot(new MakeTempCardInHandAction(card, 1));
                addToBot(new RemoveSpecificPowerAction(p, p, ForcedNPCardPower.POWER_ID));
                tickDuration();
                return;
            }
            
            NobleCardGroup<AbsNoblePhantasmCard> nobleCardGroup = new NobleCardGroup<AbsNoblePhantasmCard>();
            
            if (NobleDeckCards.nobleCards.group.isEmpty()) {
                isDone = true;
                return;
            }

            NobleDeckCards.nobleCards.group.forEach(card -> {
                AbsNoblePhantasmCard cardCopy = (AbsNoblePhantasmCard) card.makeCopy();
                if (OCAmt > 0) {
                    for (int i = 0; i < OCAmt; i++) {
                        cardCopy.upgrade();
                    }
                }
                nobleCardGroup.addToBottom(cardCopy);
                UnlockTracker.markCardAsSeen(cardCopy.cardID);
            });

            AbstractDungeon.gridSelectScreen.open(nobleCardGroup, 1, NPTEXT[2], false, false, true, false);
            tickDuration();
            return;
        }

        ArrayList<AbstractCard> selectedCards = AbstractDungeon.gridSelectScreen.selectedCards;

        if (!selectedCards.isEmpty()) {
            AbsNoblePhantasmCard selectedCard = (AbsNoblePhantasmCard) selectedCards.get(0);
            addToBot(new MakeTempCardInHandAction(selectedCard));
            selectedCards.clear();
        }
        tickDuration();
    }
}
