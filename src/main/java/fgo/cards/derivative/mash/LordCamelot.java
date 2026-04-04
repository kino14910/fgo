package fgo.cards.derivative.mash;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.CardStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ReducePercentDamagePower;

public class LordCamelot extends AbsNoblePhantasmCard {
    public static final String ID = makeID(LordCamelot.class.getSimpleName());
    public static final CardStrings cardStrings = CardCrawlGame.languagePack.getCardStrings(ID);
    
    public LordCamelot() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setCustomVar("metal", 1, 2);
        setMagic(10, 10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, customVar("metal"))));
        addToBot(new ApplyPowerAction(p, p, new ReducePercentDamagePower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 3)));
    }
}
