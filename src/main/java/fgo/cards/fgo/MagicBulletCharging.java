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

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new MagicBulletChargingAction(p, m, this.freeToPlayOnce, this.energyOnUse));
    }

    @Override
    public void applyPowers() {
        int effect = EnergyPanel.totalCount;
        if (this.energyOnUse != -1) {
            effect = this.energyOnUse;
        }
        if (AbstractDungeon.player != null && AbstractDungeon.player.hasRelic("Chemical X")) {
            effect += 2;
        }
        effect = Math.max(0, effect);

        int attackCount = 0;
        if (AbstractDungeon.player != null) {
            for (AbstractCard card : AbstractDungeon.player.hand.group) {
                if (card.type == AbstractCard.CardType.ATTACK) {
                    attackCount++;
                }
            }
        }

        int delta = effect - attackCount;
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
