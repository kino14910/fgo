package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.NPRatePower;
import fgo.powers.ReducePercentDamagePower;

public class KnightStance extends FGOCard {
    public static final String ID = makeID(KnightStance.class.getSimpleName());

    public KnightStance() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setBlock(11);
        setMagic(20, 20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new GainBlockAction(p, p, block));
        addToBot(new ApplyPowerAction(p, p, new ReducePercentDamagePower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, 1)));
    }
}


