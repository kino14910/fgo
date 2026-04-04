package fgo.powers;

import static fgo.FGOMod.makeID;
import static fgo.utils.ModHelper.getPowerCount;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.DexterityPower;
import com.megacrit.cardcrawl.powers.StrengthPower;

public class DazzlingMoonPower extends BasePower {
    public static final String POWER_ID = makeID(DazzlingMoonPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public DazzlingMoonPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount); 
    }

    @Override
    public void atStartOfTurn() {
        flash();
        addToTop(new ApplyPowerAction(owner, owner, new StrengthPower(owner, -1)));
        
        addToTop(new AllEnemyApplyPowerAction(owner, -2,
                monster -> new StrengthPower(monster, -2)));
                
        addToBot(new ReducePowerAction(owner, owner, ID, 1));
    }

    @Override
    public void onRemove() {
        int strength = getPowerCount(owner, StrengthPower.POWER_ID);
        if (strength < 0) {
            int StrAmt = -strength;
            addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, StrAmt * 2)));
            addToBot(new ApplyPowerAction(owner, owner, new DexterityPower(owner, StrAmt)));
        }
    }
}
