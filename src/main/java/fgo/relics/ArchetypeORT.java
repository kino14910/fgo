package fgo.relics;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.relics.OnReceivePowerRelic;
import com.evacipated.cardcrawl.mod.stslib.relics.SuperRareRelic;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.powers.AbstractPower;
import com.megacrit.cardcrawl.powers.BufferPower;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.AntiPurgeDefensePower;
import fgo.powers.NPRegenPower;
import fgo.powers.StarRegenPower;

public class ArchetypeORT extends BaseRelic implements SuperRareRelic, OnReceivePowerRelic {
    private static final String NAME = ArchetypeORT.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public ArchetypeORT() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.RARE, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public boolean onReceivePower(AbstractPower power, AbstractCreature p) {
        if (power.ID.equals(BufferPower.POWER_ID)) {
            flash();
            addToBot(new ApplyPowerAction(p, p, new AntiPurgeDefensePower(p, power.amount)));
            return false;
        }
        return true;
    }
    
    @Override
    public void atBattleStart() {
        AbstractPlayer p = AbstractDungeon.player;
        addToBot(new ApplyPowerAction(p, p, new StarRegenPower(p, 10)));
        addToBot(new ApplyPowerAction(p, p, new NPRegenPower(p, 3)));
    }
}
