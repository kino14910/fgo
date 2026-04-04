package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.MorningLarkPower;
import fgo.powers.StarPower;

public class MorningLark extends FGOCard {
    public static final String ID = makeID(MorningLark.class.getSimpleName());

    public MorningLark() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setNP(30, 20);
        setStar(10, 10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(np));
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
        addToBot(new ApplyPowerAction(p, p, new MorningLarkPower(p, 1)));
    }
}


