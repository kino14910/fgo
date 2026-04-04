package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.RemoveAllBlockAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ArtsPerformancePower;

public class Overload extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Overload.class.getSimpleName());

    public Overload() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 1);
        setDamage(30, 8);
        setMagic(2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new ArtsPerformancePower(p, magicNumber)));
        addToBot(new RemoveAllBlockAction(m, p));
        addToBot(new DamageAction(m, new DamageInfo(m, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.FIRE));
    }
}
