package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.StarPower;

public class DepartureOfTheSun extends FGOCard {
    public static final String ID = makeID(DepartureOfTheSun.class.getSimpleName());

    public DepartureOfTheSun() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.RARE);
        setMagic(20, 10);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (p.hasPower(StarPower.POWER_ID)) {
            addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, p.getPower(StarPower.POWER_ID).amount / 10 * magicNumber)));
        }
    }
}


