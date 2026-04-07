package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;
import fgo.powers.PursuerOfLovePower;

public class PursuerofLove extends FGOCard {
    public static final String ID = makeID(PursuerofLove.class.getSimpleName());

    public PursuerofLove() {
        super(ID, 0, CardType.SKILL, CardTarget.ENEMY, CardRarity.COMMON);
        setMagic(3, 2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(m, p, new StrengthPower(m, 1)));
        addToBot(new ApplyPowerAction(m, p, new PursuerOfLovePower(m, magicNumber)));
    }
}


