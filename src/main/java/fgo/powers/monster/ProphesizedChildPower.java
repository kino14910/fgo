package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.action.FgoNpAction;
import fgo.powers.BasePower;

public class ProphesizedChildPower extends BasePower {
    public static final String POWER_ID = makeID(ProphesizedChildPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private int turns;

    public ProphesizedChildPower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "DelayedBuffPower");
        this.turns = 0;
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0] + (this.turns == 2 
            ? DESCRIPTIONS[2] 
            : String.format(DESCRIPTIONS[1], this.turns));
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        flash();
        if (this.turns == 2) {
            addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, 2), 2));
            addToBot(new FgoNpAction(30));
            this.turns = 0;
        } else {
            ++this.turns;
        }
        this.updateDescription();
    }
}
