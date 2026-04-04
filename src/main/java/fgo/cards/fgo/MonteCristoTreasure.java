package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.MonteCristoTreasurePower;

public class MonteCristoTreasure extends FGOCard {
    public static final String ID = makeID(MonteCristoTreasure.class.getSimpleName());

    public MonteCristoTreasure() {
        super(ID, 3, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(1);
        setCostUpgrade(2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new MonteCristoTreasurePower(p, magicNumber)));
    }
}


