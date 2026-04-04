package fgo.cards.optionCards;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class Kaleidoscope extends FGOCard {
    public static final String ID = makeID(Kaleidoscope.class.getSimpleName());

    public Kaleidoscope() {
        super(ID, -2, CardType.POWER, CardTarget.SELF, CardRarity.SPECIAL, CardColor.COLORLESS);
        setNP(40, 20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
    }

    @Override
    public void onChoseThisOption() {
        addToBot(new FgoNpAction(np));
    }
}
