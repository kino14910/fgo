package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.watcher.VigorPower;

public class SunlightPower extends BasePower {
    public static final String POWER_ID = makeID(SunlightPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public SunlightPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount);
    }
    
    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        if (card.type == AbstractCard.CardType.ATTACK) {
            flash();
            gainVigor();
        }
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        flash();
        gainVigor();
        addToBot(new ReducePowerAction(owner, owner, ID, 1));
        if (amount == 1) {
            addToBot(new ReducePowerAction(owner, owner, CriticalDamageUpPower.POWER_ID, 50));
        } 
    }
    
    private void gainVigor() {
        if (owner.hasPower(VigorPower.POWER_ID)) {
            int vigorAmt = owner.getPower(VigorPower.POWER_ID).amount;
            addToBot(new ApplyPowerAction(owner, owner, new VigorPower(owner, vigorAmt)));
        }
    }

    @Override
    public void updateDescription() {
        description = DESCRIPTIONS[0];
    }
}
