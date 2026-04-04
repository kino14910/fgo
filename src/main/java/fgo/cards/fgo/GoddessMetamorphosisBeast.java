package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.GoddessMetamorphosisBeastPower;

public class GoddessMetamorphosisBeast extends FGOCard {
    public static final String ID = makeID(GoddessMetamorphosisBeast.class.getSimpleName());

    public GoddessMetamorphosisBeast() {
        super(ID, 2, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(1);
        setEthereal(true, false);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GoddessMetamorphosisBeastPower(p, magicNumber)));
    }
}


