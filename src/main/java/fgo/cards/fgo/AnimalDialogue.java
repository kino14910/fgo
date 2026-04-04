package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.characters.Master;

public class AnimalDialogue extends FGOCard {
    public static final String ID = makeID(AnimalDialogue.class.getSimpleName());

    public AnimalDialogue() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(20);
        setCostUpgrade(0);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (Master.fgoNp >= magicNumber) {
            addToBot(new GainEnergyAction(Master.fgoNp / magicNumber));
            addToBot(new FgoNpAction(-Master.fgoNp));
        }
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = Master.fgoNp >= magicNumber
            ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy() 
            : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }
}

