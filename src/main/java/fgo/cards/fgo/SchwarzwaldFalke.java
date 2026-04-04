package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.NPRatePower;
import fgo.powers.SchwarzwaldFalkePower;

public class SchwarzwaldFalke extends FGOCard {
    public static final String ID = makeID(SchwarzwaldFalke.class.getSimpleName());

    public SchwarzwaldFalke() {
        super(ID, 3, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(3);
        setCostUpgrade(2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new SchwarzwaldFalkePower(p, magicNumber)));
    }
}


