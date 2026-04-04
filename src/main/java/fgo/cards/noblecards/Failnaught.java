package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;

public class Failnaught extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Failnaught.class.getSimpleName());

    public Failnaught() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 2);
        setDamage(48, 12);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new LoseHPAction(m, p, damage));
    }
}
