package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.NonStackablePower;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.patches.RevivePatch;

public class NonStackableGutsPower extends BasePower implements NonStackablePower {
    public static final String POWER_ID = makeID(NonStackableGutsPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private int time;
 
    public NonStackableGutsPower(AbstractCreature owner, int amount, int time) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, GutsPower.class.getSimpleName());
        this.time = time;
        amount2 = time == 1 ? 0 : time;
        updateDescription();
    }

    /**
     * {@link RevivePatch}
     */
    @Override
    public void onSpecificTrigger() {
        AbstractDungeon.player.heal(Math.max(amount, 1), true);
        time--;
        amount2 = time == 1 ? 0 : time;
        if (time == 0) {
            addToTop(new RemoveSpecificPowerAction(owner, owner, ID));
        } 
        
        updateDescription();
    }
    
    @Override
    public void updateDescription() {
        description = time == 1 ? String.format(DESCRIPTIONS[0], amount) : String.format(DESCRIPTIONS[1], time, amount);
    }
}