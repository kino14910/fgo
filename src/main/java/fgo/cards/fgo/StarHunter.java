package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.StarHunterPower;
import fgo.powers.StarPower;

public class StarHunter extends FGOCard {
    public static final String ID = makeID(StarHunter.class.getSimpleName());

    public StarHunter() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(50, 50);
        setStar(8);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
        addToBot(new ApplyPowerAction(p, p, new StarHunterPower(p, 3, magicNumber)));
    }
}


