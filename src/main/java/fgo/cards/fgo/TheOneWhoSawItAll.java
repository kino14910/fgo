package fgo.cards.fgo;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.TheOneWhoSawItAllAction;
import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;

public class TheOneWhoSawItAll extends FGOCard {
    public static final String ID = makeID(TheOneWhoSawItAll.class.getSimpleName());

    public TheOneWhoSawItAll() {
        super(ID, 0, CardType.SKILL, CardTarget.NONE, CardRarity.UNCOMMON, FGOCardColor.FGO);
        setMagic(4, 2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new TheOneWhoSawItAllAction(1, CardType.ATTACK, magicNumber));
    }
}


