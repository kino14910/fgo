package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.DrawCardAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class SwordOfSelection extends FGOCard {
    public static final String ID = makeID(SwordOfSelection.class.getSimpleName());

    public SwordOfSelection() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setMagic(3, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DrawCardAction(p, magicNumber));
        addToBot(new FgoNpAction(np, true));
    }

    @Override
    public void applyPowers() {
        rawDescription = cardStrings.DESCRIPTION + cardStrings.EXTENDED_DESCRIPTION[0];
        super.applyPowers();
        int finalNp = AbstractDungeon.player.hand.size() + magicNumber - 1;
        if (finalNp > 10) {
            finalNp = 10;
        }
        setNP(finalNp * 2);
        
        initializeDescription();
    }
}


