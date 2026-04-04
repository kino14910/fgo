package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.NonStackablePower;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.action.FgoNpAction;

public class NonStackableEnergyRegenPower extends BasePower implements NonStackablePower {
    public static final String POWER_ID = makeID(NonStackableEnergyRegenPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private int time;
 
    public NonStackableEnergyRegenPower(AbstractCreature owner, int amount, int time) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, EnergyRegenPower.class.getSimpleName());
        this.time = time;
        amount2 = time == 1 ? 0 : time;
        updateDescription();
    }

    @Override
    public void atStartOfTurn() {
        addToBot(new FgoNpAction(amount));
        time--;
        amount2 = time == 1 ? 0 : time;
        if (time == 0) {
            addToTop(new RemoveSpecificPowerAction(owner, owner, ID));
        } 
        
        updateDescription();
    }
    
    @Override
    public void updateDescription() {
        description = String.format(DESCRIPTIONS[0], amount) + String.format(DESCRIPTIONS[1], time);
    }
}