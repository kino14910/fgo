package fgo.action;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.utility.NewQueueCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.ui.panels.EnergyPanel;

public class MagicBulletChargingAction extends AbstractGameAction {
    private AbstractPlayer p;
    private AbstractMonster target;
    private boolean freeToPlayOnce;
    private int energyOnUse;

    public MagicBulletChargingAction(AbstractPlayer p, AbstractMonster target, final boolean freeToPlayOnce, final int energyOnUse) {
        this.p = p;
        this.target = target;
        this.freeToPlayOnce = freeToPlayOnce;
        this.energyOnUse = energyOnUse;
        this.duration = Settings.ACTION_DUR_XFAST;
        actionType = ActionType.CARD_MANIPULATION;
    }

    @Override
    public void update() {
        int effect = EnergyPanel.totalCount;
        if (energyOnUse != -1) {
            effect = energyOnUse;
        }
        if (this.p.hasRelic("Chemical X")) {
            effect += 2;
            this.p.getRelic("Chemical X").flash();
        }
        effect = Math.max(0, effect);

        int attackCount = 0;
        CardGroup hand = p.hand;
        for (AbstractCard card : hand.group) {
            if (card.type == AbstractCard.CardType.ATTACK) {
                attackCount++;
            }
        }

        if (attackCount <= 0) {
            isDone = true;
            return;
        }

        
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 5)));

        int cardsToPlay = Math.min(effect, attackCount);
        int delta = effect - attackCount;
        int cardsPlayed = 0;
        int handSize = hand.size();
        
        for (int i = 0; i < handSize && cardsPlayed < cardsToPlay; i++) {
            AbstractCard card = hand.group.get(i);
            
            if (card.type == AbstractCard.CardType.ATTACK) {
                cardsPlayed++;
                
                boolean isLastAttack = (cardsPlayed == cardsToPlay);
                
                if (isLastAttack && delta > 0) {
                    card.damage += (delta * 5);
                    card.isDamageModified = true;
                }
                
                card.calculateCardDamage(this.target);
                addToBot(new NewQueueCardAction(card, this.target, false, true));
            }
        }

                addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, -5)));

        if (!freeToPlayOnce) {
            p.energy.use(EnergyPanel.totalCount);
        }

        isDone = true;
    }
}
