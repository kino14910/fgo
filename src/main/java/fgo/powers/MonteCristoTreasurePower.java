package fgo.powers;

import static fgo.FGOMod.makeID;
import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class MonteCristoTreasurePower extends BasePower {
    public static final String POWER_ID = makeID(MonteCristoTreasurePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public MonteCristoTreasurePower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "FightToDeathPower");
    }

    @Override
    public void updateDescription() {
        description = formatDescriptionByQuantity(amount);
    }

    @Override
    public void onAttack(DamageInfo info, int damageAmount, AbstractCreature target) {
        if (damageAmount > 0 
                && target != owner 
                && info.type == DamageInfo.DamageType.NORMAL 
                && getPowerCount(owner, StarPower.POWER_ID) >= 10) {
            flash();
            addToBot(new GainBlockAction(owner, owner, damageAmount * amount));
        }
    }
}
