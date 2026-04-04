package fgo.cards.fgo;

import com.evacipated.cardcrawl.mod.stslib.actions.tempHp.AddTemporaryHPAction;
import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class Coaching extends FGOCard {
    public static final String ID = makeID(Coaching.class.getSimpleName());

    public Coaching() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(3, 6);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new GainEnergyAction(2));
        addToBot(new LoseHPAction(p, p, 3));
        addToBot(new AddTemporaryHPAction(p, p, magicNumber));
    }
}

