package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.LoseStrengthPower;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.LoseCritDamagePower;

public class HeroCreation extends FGOCard {
    public static final String ID = makeID(HeroCreation.class.getSimpleName());

    public HeroCreation() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setMagic(2, 2);
        setCustomVar("CriticalDamage", 50, 50);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new LoseStrengthPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, customVar("CriticalDamage"))));
        addToBot(new ApplyPowerAction(p, p, new LoseCritDamagePower(p, customVar("CriticalDamage"))));
    }
}


