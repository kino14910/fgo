package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.NonStackablePower;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.action.FgoNpAction;

public class NonStackableNPRegenPower extends BasePower implements NonStackablePower {
    public static final String POWER_ID = makeID(NonStackableNPRegenPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private int time;

    public NonStackableNPRegenPower(AbstractCreature owner, int amount, int time) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, NPRegenPower.class.getSimpleName());
        this.time = time;
        amount2 = time == 1 ? 0 : time;
        updateDescription();
    }

    @Override
    public void atStartOfTurn() {
        flash();
        addToBot(new FgoNpAction(amount));
        if (time == 1) {
            addToTop(new RemoveSpecificPowerAction(owner, owner, ID));
        } else {
            time--;
            if (time == 0) {
                time = 0;
            }
        }
    }
}
