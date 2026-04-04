package fgo.cards.fgo;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.characters.Master;

public class Borrowingfrom extends FGOCard {
    public static final String ID = makeID(Borrowingfrom.class.getSimpleName());

    public Borrowingfrom() {
        super(ID, 2, CardType.SKILL, CardTarget.NONE, CardRarity.RARE);
        setCostUpgrade(1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(Master.fgoNp));
    }
}


