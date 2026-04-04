package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ElementaryPower;

public class ElementaryMyDear extends AbsNoblePhantasmCard {
    public static final String ID = makeID(ElementaryMyDear.class.getSimpleName());

    public ElementaryMyDear() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setMagic(1, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new ElementaryPower(p, magicNumber)));
    }
}
