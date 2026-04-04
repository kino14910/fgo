package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.NPDamagePower;
import fgo.powers.NPOverChargePower;

public class DreamUponTheStars extends FGOCard {
    public static final String ID = makeID(DreamUponTheStars.class.getSimpleName());

    public DreamUponTheStars() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.BASIC);
        setMagic(20, 10);
        setNP(10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(np));
        addToBot(new ApplyPowerAction(p, p, new NPDamagePower(magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(1)));
    }
}


