package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.EnergyRegenPower;

public class DesiresSalvation extends FGOCard {
    public static final String ID = makeID(DesiresSalvation.class.getSimpleName());

    public DesiresSalvation() {
        super(ID, 1, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setNP(10);
        setInnate(false, true);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new EnergyRegenPower(p, np)));
    }
}


