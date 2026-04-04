package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.NonStackableGutsPower;
import fgo.powers.SpringOfFirePower;

public class SpringOfFire extends FGOCard {
    public static final String ID = makeID(SpringOfFire.class.getSimpleName());

    public SpringOfFire() {
        super(ID, 3, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(3, 3);
        tags.add(CardTags.HEALING);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NonStackableGutsPower(p, magicNumber, 3)));
        addToBot(new ApplyPowerAction(p, p, new SpringOfFirePower(p, 20)));
    }
}


