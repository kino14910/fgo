package fgo.cards.fgo;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.DragonCoreAction;
import fgo.cards.FGOCard;

public class DragonCore extends FGOCard {
    public static final String ID = makeID(DragonCore.class.getSimpleName());

    public DragonCore() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DragonCoreAction(upgraded));
    }
}


