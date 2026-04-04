package fgo.cards.fgo;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.TameshiMonoAction;
import fgo.cards.FGOCard;

public class TameshiMono extends FGOCard {
    public static final String ID = makeID(TameshiMono.class.getSimpleName());

    public TameshiMono() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(2, 1);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new TameshiMonoAction(magicNumber));
    }
}


