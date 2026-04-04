package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.FragarachPower;

public class Fragarach extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Fragarach.class.getSimpleName());

    public Fragarach() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setDamage(15, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new FragarachPower(p, damage)));
    }
}
