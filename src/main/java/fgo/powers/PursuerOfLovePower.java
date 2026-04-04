package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class PursuerOfLovePower extends BasePower {
    public static final String POWER_ID = makeID(PursuerOfLovePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    
    public PursuerOfLovePower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, Math.min(amount, 100), "DefenseDownPower");
    }

    @Override
    public void updateDescription() {
        description = formatDescriptionByQuantity(amount);
    }

    @Override
    public float atDamageReceive(float damage, DamageInfo.DamageType type) {
        return damage * 1.25f;
    }

    @Override
    public void atEndOfRound() {
        addToBot(new ReducePowerAction(owner, owner, this, 1));
    }
}
