package fgo.relics.deprecated;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.relics.OnReceivePowerRelic;
import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.CursePower;
import fgo.relics.BaseRelic;

public class MechanicalProtector extends BaseRelic implements OnReceivePowerRelic {
    private static final String NAME = "MechanicalProtector";
	public static final String ID = makeID(NAME);
    
    public MechanicalProtector() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.RARE, LandingSound.FLAT);
    }
    
    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public boolean onReceivePower(AbstractPower power, AbstractCreature p) {
        if (power.ID.equals(CursePower.POWER_ID)) {
            addToBot(new HealAction(p, p, 1));
        }
        return true;
    }
}
