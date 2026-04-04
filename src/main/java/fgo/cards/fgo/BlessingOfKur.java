package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.cards.noblecards.KurKigalIrkalla;
import fgo.powers.BlessingOfKurPower;
import fgo.powers.NPRatePower;

public class BlessingOfKur extends FGOCard {
    public static final String ID = makeID(BlessingOfKur.class.getSimpleName());

    public BlessingOfKur() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(6, 9);
        cardsToPreview = new KurKigalIrkalla();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, 3)));
        addToBot(new ApplyPowerAction(p, p, new BlessingOfKurPower(p, magicNumber, 2)));
    }
}


