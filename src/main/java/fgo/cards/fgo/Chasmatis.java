package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.NPOverChargePower;

public class Chasmatis extends FGOCard {
    public static final String ID = makeID(Chasmatis.class.getSimpleName());

    public Chasmatis() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(1);
        setCostUpgrade(0);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(magicNumber)));
    }
}


