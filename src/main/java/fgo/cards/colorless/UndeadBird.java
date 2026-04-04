package fgo.cards.colorless;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.RandomCardWithTagAction;
import fgo.cards.FGOCard;

public class UndeadBird extends FGOCard {
    public static final String ID = makeID(UndeadBird.class.getSimpleName());

    public UndeadBird() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON, CardColor.COLORLESS);
        setMagic(1, 1);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (int i = 0; i < this.magicNumber; ++i) {
            addToBot(new RandomCardWithTagAction(false, CardTags.HEALING, false, false));
        }
    }
}


