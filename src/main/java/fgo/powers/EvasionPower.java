package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class EvasionPower extends BasePower {
    public static final String POWER_ID = makeID(EvasionPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public EvasionPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount); 
    }

    @Override
    public void updateDescription() {
        description = formatDescriptionByQuantity(amount);
    }

    @Override
    public void stackPower(int stackAmount) {
        fontScale = 8.0f;
        amount += stackAmount;
    }

    @Override
    public int onAttackedToChangeDamage(DamageInfo info, int damageAmount) {
        if (damageAmount > 0 && info.type == DamageInfo.DamageType.NORMAL) {
            addToTop(new ReducePowerAction(owner, owner, ID, 1));
            return 0;
        }

        return damageAmount;
    }
}
