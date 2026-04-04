package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.DexterityPower;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;
import fgo.powers.MyFairSoldierPower;

public class QueensCovenant extends FGOCard {
    public static final String ID = makeID(QueensCovenant.class.getSimpleName());

    public QueensCovenant() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(3, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new DexterityPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new MyFairSoldierPower(p, magicNumber)));
    }
}


