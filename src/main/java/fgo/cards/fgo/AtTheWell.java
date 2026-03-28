package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.unique.RemoveAllPowersAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.AtTheWellPower;

public class AtTheWell extends FGOCard {
    public static final String ID = makeID(AtTheWell.class.getSimpleName());

    public AtTheWell() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.RARE);
        setMagic(6, 12);
        setExhaust();
        tags.add(CardTags.HEALING);
    }

    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new RemoveAllPowersAction(p, true));
        addToBot(new ApplyPowerAction(p, p, new AtTheWellPower(p, magicNumber)));
    }
}

