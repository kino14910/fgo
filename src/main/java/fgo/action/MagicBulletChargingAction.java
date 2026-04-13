package fgo.action;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.utility.NewQueueCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.ui.panels.EnergyPanel;

import fgo.cards.fgo.MagicBulletCharging;

public class MagicBulletChargingAction extends AbstractGameAction {
    private AbstractPlayer p;
    private AbstractMonster target;
    private boolean freeToPlayOnce;
    private int energyOnUse;
    private int effect;
    private int delta;

    public MagicBulletChargingAction(AbstractPlayer p, AbstractMonster target, boolean freeToPlayOnce, int energyOnUse, int effect, int delta) {
        this.p = p;
        this.target = target;
        this.freeToPlayOnce = freeToPlayOnce;
        this.energyOnUse = energyOnUse;
        this.effect = effect;
        this.delta = delta;
        this.duration = Settings.ACTION_DUR_XFAST;
        actionType = ActionType.CARD_MANIPULATION;
    }

    @Override
    public void update() {
        int attackCount = MagicBulletCharging.countAttacks(p);

        if (attackCount <= 0) {
            isDone = true;
            return;
        }

        int cardsToPlay = Math.min(effect, attackCount);
        int cardsPlayed = 0;
        CardGroup hand = p.hand;
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
                addToTop(new NewQueueCardAction(card, this.target, false, true));
            }
        }

        if (!freeToPlayOnce) {
            p.energy.use(EnergyPanel.totalCount);
        }

        isDone = true;
    }
}
