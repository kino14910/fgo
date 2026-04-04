package fgo.cards.optionCards;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class ReleaseNoblePhantasm extends FGOCard {
    public static final String ID = makeID(ReleaseNoblePhantasm.class.getSimpleName());

    public ReleaseNoblePhantasm() {
        super(ID, -2, CardType.POWER, CardTarget.NONE, CardRarity.SPECIAL, CardColor.COLORLESS, cardPath("power/CommandSpellGuts"));
        setNP(100);
    }

    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) { onChoseThisOption(); }

    @Override
    public void onChoseThisOption() {
        addToBot(new FgoNpAction(np));
    }
}
