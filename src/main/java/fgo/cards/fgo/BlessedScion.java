package fgo.cards.fgo;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.BlessedScionAction;
import fgo.cards.FGOCard;

public class BlessedScion extends FGOCard {
    public static final String ID = makeID(BlessedScion.class.getSimpleName());

    public BlessedScion() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(1, 1);
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new BlessedScionAction(p, p, magicNumber));
    }
}


