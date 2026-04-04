package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.StarPower;

public class BridalSpinWheel extends AbsNoblePhantasmCard {
    public static final String ID = makeID(BridalSpinWheel.class.getSimpleName());

    public BridalSpinWheel() {
        super(ID, CardType.SKILL, CardTarget.SELF, 1);
        setMagic(6);
        setStar(20, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
    }
}
