package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.NPDamagePower;
import fgo.powers.WatersidePower;

public class WaterfrontSaintess extends FGOCard {
    public static final String ID = makeID(WaterfrontSaintess.class.getSimpleName());
    
    public WaterfrontSaintess() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setNP(20, 20);
        setMagic(10, 10);
        setCustomVar("CriticalDamage", 30, 20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(np));
        addToBot(new ApplyPowerAction(p, p, new NPDamagePower(magicNumber)));
        if (p.hasPower(WatersidePower.POWER_ID)) {
            addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, customVar("CriticalDamage"))));
        }
    }
}


