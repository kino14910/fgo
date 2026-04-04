package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.IndomitablePower;
import fgo.powers.NonStackableGutsPower;

public class Indomitable extends FGOCard {
    public static final String ID = makeID(Indomitable.class.getSimpleName());

    public Indomitable() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(5, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NonStackableGutsPower(p, magicNumber, 1)));
        addToBot(new ApplyPowerAction(p, p, new IndomitablePower(p, 2)));
    }
}


