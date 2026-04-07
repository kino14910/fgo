package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.EndOfADreamPower;
import fgo.powers.PowerUpBoostPower;

public class EndOfADream extends FGOCard {
    public static final String ID = makeID(EndOfADream.class.getSimpleName());

    public EndOfADream() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new PowerUpBoostPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new EndOfADreamPower(p, upgraded)));
    }
}


