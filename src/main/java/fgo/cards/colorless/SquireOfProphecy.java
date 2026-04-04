package fgo.cards.colorless;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.SquireOfProphecyPower;

public class SquireOfProphecy extends FGOCard {
    public static final String ID = makeID(SquireOfProphecy.class.getSimpleName());

    public SquireOfProphecy() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON, CardColor.COLORLESS);
        setMagic(2, 1);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new SquireOfProphecyPower(p, magicNumber)));
    }
}


