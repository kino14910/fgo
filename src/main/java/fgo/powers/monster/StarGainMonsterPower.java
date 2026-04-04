package fgo.powers.monster;

import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.powers.BasePower;
import fgo.powers.CriticalDamageUpPower;

public class StarGainMonsterPower extends BasePower {
    public static final String POWER_ID = StarGainMonsterPower.class.getSimpleName();
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public StarGainMonsterPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "StarPower");
    }

    @Override
    public void updateDescription() {description = DESCRIPTIONS[0];}

    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type, AbstractCard card) {
        if (amount >= 10) {
            return atDamageGive(damage, type);
        }
        return damage;
    }

    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type) {
        return type == DamageInfo.DamageType.NORMAL ? damage * (200.0f + getPowerCount(owner, CriticalDamageUpPower.POWER_ID)) / 100.0f : damage;
    }

    @Override
    public void onAttack(DamageInfo info, int damageAmount, AbstractCreature target) {
       flash();
       addToTop(new ReducePowerAction(owner, owner, ID, 10));
    }
}
