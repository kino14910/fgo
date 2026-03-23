package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.AbstractPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.powers.VulnerablePower;

import fgo.powers.BasePower;

public class FaeQueenPower extends BasePower {
    public static final String POWER_ID = makeID(FaeQueenPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    
    private int turns;

    public FaeQueenPower(AbstractCreature owner) {
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
    public void atEndOfTurnPreEndTurnCards(boolean isPlayer) {
        flash();
        if (this.turns == 2) {
            addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, 2), 2));
            addToBot(new ApplyPowerAction(AbstractDungeon.player, owner, new VulnerablePower(AbstractDungeon.player, 1, true), 1));
            this.turns = 0;
        } else {
            ++this.turns;
        }
        for (AbstractPower power : this.owner.powers) {
            if (power.type != AbstractPower.PowerType.DEBUFF) continue;
            addToBot(new RemoveSpecificPowerAction(owner, owner, power));
            break;
        }
        this.updateDescription();
    }
}
