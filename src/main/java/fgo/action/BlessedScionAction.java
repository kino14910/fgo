package fgo.action;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.actions.common.SelectCardsInHandAction;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInHandAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.UIStrings;

public class BlessedScionAction extends AbstractGameAction {
    private static final UIStrings uiStrings = CardCrawlGame.languagePack.getUIString(makeID(BlessedScionAction.class.getSimpleName()));
    public static final String[] TEXT = uiStrings.TEXT;
    
    public BlessedScionAction(AbstractCreature target, AbstractCreature source, int amount) {
        setValues(target, source, amount);
        actionType = ActionType.CARD_MANIPULATION;
    }

    @Override
    public void update() {
        addToBot(new SelectCardsInHandAction(amount, TEXT[0], cards -> 
            cards.forEach(card -> addToBot(new MakeTempCardInHandAction(processCard(card), 1, false)))));
            isDone = true;
    }

    private AbstractCard processCard(AbstractCard card) {
        AbstractCard newCard = card.makeCopy();
        if (newCard.costForTurn >= 0) {
            newCard.freeToPlayOnce = true;
        }
        return newCard;
    }
}
