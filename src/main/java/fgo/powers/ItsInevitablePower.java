package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class ItsInevitablePower extends BasePower {
    public static final String POWER_ID = makeID(ItsInevitablePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    public int boost = 0;
    
    public ItsInevitablePower(AbstractCreature owner, int amount, int boost) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "BurningPower");
        amount2 += boost;
        this.boost = boost;
        updateDescription();
    }
    
    @Override
    public void stackPower(int stackAmount) {
        super.stackPower(stackAmount);
        amount2 += boost;
    }

    @Override
    public void updateDescription() {
        description = String.format(DESCRIPTIONS[0], amount, amount2);
    }

    @Override
    public void atStartOfTurn() {
        flash();
        amount += amount2;
        addToBot(new DamageAllEnemiesAction(owner, DamageInfo.createDamageMatrix(amount, true),
                DamageInfo.DamageType.NORMAL, AbstractGameAction.AttackEffect.FIRE));
        updateDescription();
    }
}
