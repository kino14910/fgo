package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.DeathOfDeathPower;
import fgo.powers.GutsPower;

public class AbyssOfDeath extends FGOCard {
    public static final String ID = makeID(AbyssOfDeath.class.getSimpleName());

    public AbyssOfDeath() {
        super(ID, 2, CardType.POWER, CardTarget.SELF, CardRarity.RARE);
        setMagic(15, 10);
        this.tags.add(CardTags.HEALING);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GutsPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new DeathOfDeathPower(p)));
    }
}


