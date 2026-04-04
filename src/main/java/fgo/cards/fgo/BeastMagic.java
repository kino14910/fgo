package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.BeastMagicPower;

public class BeastMagic extends FGOCard {
    public static final String ID = makeID(BeastMagic.class.getSimpleName());

    public BeastMagic() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(3, 1);
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new BeastMagicPower(p, magicNumber)));
    }
}


