package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.watcher.VigorPower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.SunlightPower;

public class ExcaliburGalatine extends AbsNoblePhantasmCard {
    public static final String ID = makeID(ExcaliburGalatine.class.getSimpleName());

    public ExcaliburGalatine() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(24, 6);
        setMagic(2, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.SLASH_HORIZONTAL));
        addToBot(new ApplyPowerAction(p, p, new SunlightPower(p, 3)));
        addToBot(new ApplyPowerAction(p, p, new VigorPower(p, magicNumber)));
        if (!p.hasPower(SunlightPower.POWER_ID)) {
            addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, 50)));
        }
    }
}
