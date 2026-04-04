package fgo.cards.derivative.mash;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.CardStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ReducePercentDamagePower;

public class Camelot extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Camelot.class.getSimpleName());
    public static final CardStrings cardStrings = CardCrawlGame.languagePack.getCardStrings(ID);
    
    public Camelot() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setCustomVar("metal", 1, 2);
        setBlock(2, 5);
        setMagic(10, 10);
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, customVar("metal"))));
        addToBot(new ApplyPowerAction(p, p, new ReducePercentDamagePower(p, magicNumber)));
    }
}
