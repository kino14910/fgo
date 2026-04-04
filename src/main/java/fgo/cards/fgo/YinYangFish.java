package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class YinYangFish extends FGOCard {
    public static final String ID = makeID(YinYangFish.class.getSimpleName());

    public YinYangFish() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setNP(10);
        setMagic(6);
        tags.add(CardTags.HEALING);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (upgraded) {
            addToBot(new FgoNpAction(30));
        }
        addToBot(new FgoNpAction(-np));
        addToTop(new HealAction(p, p, magicNumber));
    }
}


