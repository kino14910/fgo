package fgo.cards.colorless;

import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.FleetingField;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class Dumuzid extends FGOCard {
    public static final String ID = makeID(Dumuzid.class.getSimpleName());

    public Dumuzid() {
        super(ID, 4, CardType.STATUS, CardTarget.SELF, CardRarity.SPECIAL, CardColor.COLORLESS);
        FleetingField.fleeting.set(this, true);
    }
    
    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
    }
}


