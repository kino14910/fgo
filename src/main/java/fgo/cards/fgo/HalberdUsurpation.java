package fgo.cards.fgo;

import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.watcher.ExpungeVFXAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.GainStrengthPower;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;

public class HalberdUsurpation extends FGOCard {
    public static final String ID = makeID(HalberdUsurpation.class.getSimpleName());

    public HalberdUsurpation() {
        super(ID, 2, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(15, 5);
    }

    @Override
    public void calculateCardDamage(AbstractMonster mo) {
        int StrengthAmount = getPowerCount(mo, StrengthPower.POWER_ID);

        int realBaseDamage = baseDamage;
        baseDamage += StrengthAmount;
        super.calculateCardDamage(mo);
        baseDamage = realBaseDamage;
        isDamageModified = damage != baseDamage;
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ExpungeVFXAction(m));
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn)));
        if (m == null || m.isDeadOrEscaped()) {
            return;
        }
        int HuAmt = getPowerCount(m, StrengthPower.POWER_ID) * 2;
        if (HuAmt <= 0) {
            return;
        }
        addToBot(new ApplyPowerAction(m, p, new StrengthPower(m, -HuAmt)));
        if (!m.hasPower("Artifact")) {
            addToBot(new ApplyPowerAction(m, p, new GainStrengthPower(m, HuAmt)));
        }
    }
}


