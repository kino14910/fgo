package fgo.cards.fgo;

import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.ui.panels.EnergyPanel;

import fgo.action.MagicBulletChargingAction;
import fgo.cards.FGOCard;

public class MagicBulletCharging extends FGOCard {
    public static final String ID = makeID(MagicBulletCharging.class.getSimpleName());

    public MagicBulletCharging() {
        super(ID, -1, CardType.SKILL, CardTarget.ENEMY, CardRarity.RARE);
    }

    public static int calculateEffect(AbstractPlayer p, int energyOnUse) {
        int effect = EnergyPanel.totalCount;
        if (energyOnUse != -1) {
            effect = energyOnUse;
        }
        if (p != null && p.hasRelic("Chemical X")) {
            effect += 2;
        }
        return Math.max(0, effect);
    }

    public static int countAttacks(AbstractPlayer p) {
        if (p == null) return 0;
        int count = 0;
        for (AbstractCard card : p.hand.group) {
            if (card.type == AbstractCard.CardType.ATTACK) {
                count++;
            }
        }
        return count;
    }

    public static int calculateDelta(int effect, int attackCount) {
        return effect - attackCount;
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        int effect = calculateEffect(p, this.energyOnUse);
        int attackCount = countAttacks(p);
        int delta = calculateDelta(effect, attackCount);
        
        addToBot(new MagicBulletChargingAction(p, m, this.freeToPlayOnce, this.energyOnUse, effect, delta));
    }

    @Override
    public void applyPowers() {
        int effect = calculateEffect(AbstractDungeon.player, this.energyOnUse);
        int attackCount = countAttacks(AbstractDungeon.player);
        int delta = calculateDelta(effect, attackCount);

        if (delta >= 1) {
            rawDescription = cardStrings.DESCRIPTION + String.format(cardStrings.EXTENDED_DESCRIPTION[0], delta * 5);
        } else {
            rawDescription = cardStrings.DESCRIPTION;
        }
        super.applyPowers();
        initializeDescription();
    }

    @Override
    public void onMoveToDiscard() {
        rawDescription = cardStrings.DESCRIPTION;
        initializeDescription();
    }
}
