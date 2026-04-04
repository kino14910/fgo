package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.FacelessMoonPower;

public class FacelessMoon extends FGOCard {
    public static final String ID = makeID(FacelessMoon.class.getSimpleName());

    public FacelessMoon() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setMagic(1);
        setSelfRetain(false, true);
        tags.add(FGO_Foreigner);
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new FacelessMoonPower(p, 1)));
    }
}


