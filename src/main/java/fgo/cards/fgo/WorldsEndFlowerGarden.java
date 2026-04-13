package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.WorldsEndFlowerGardenPower;

public class WorldsEndFlowerGarden extends FGOCard {
    public static final String ID = makeID(WorldsEndFlowerGarden.class.getSimpleName());

    public WorldsEndFlowerGarden() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new WorldsEndFlowerGardenPower(p, 10)));
    }
}
