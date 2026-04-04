package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.unique.RemoveDebuffsAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.AntiPurgeDefensePower;

public class AroundCaliburn extends AbsNoblePhantasmCard {
    public static final String ID = makeID(AroundCaliburn.class.getSimpleName());

    public AroundCaliburn() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setMagic(1, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 3)));
        addToBot(new RemoveDebuffsAction(p));
        addToBot(new ApplyPowerAction(p, p, new AntiPurgeDefensePower(p, magicNumber)));
    }
}
