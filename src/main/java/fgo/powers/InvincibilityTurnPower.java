package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class InvincibilityTurnPower extends BasePower {
    public static final String POWER_ID = makeID(InvincibilityTurnPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public InvincibilityTurnPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "InvincibilityPower");
    }

    @Override
    public void updateDescription() {
        description = formatDescriptionByQuantity(amount);
    }

    @Override
    public int onAttackedToChangeDamage(DamageInfo info, int damageAmount) {
        if (damageAmount > 0 && info.type == DamageInfo.DamageType.NORMAL) {
            return 0;
        }

        return damageAmount;
    }

    @Override
    public void atStartOfTurn() {
        addToBot(new ReducePowerAction(owner, owner, ID, 1));
    }
}
