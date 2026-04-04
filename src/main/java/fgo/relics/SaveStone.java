package fgo.relics;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.relics.SuperRareRelic;
import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;

import fgo.action.FgoNpAction;
import fgo.characters.CustomEnums.FGOCardColor;

public class SaveStone extends BaseRelic implements SuperRareRelic {
    private static final String NAME = SaveStone.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public SaveStone() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.RARE, LandingSound.HEAVY);
    }

    @Override
    public void setCounter(final int setCounter) {
        if (setCounter == -2) {
            this.usedUp();
            this.counter = -2;
        }
    }
    
    @Override
    public void onTrigger() {
        this.flash();
        AbstractPlayer p = AbstractDungeon.player;
        this.addToTop(new RelicAboveCreatureAction(p, this));
        int healAmt = AbstractDungeon.player.maxHealth;
        if (healAmt < 1) {
            healAmt = 1;
        }
        p.heal(healAmt, true);
        addToBot(new FgoNpAction(300));
        this.setCounter(-2);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }
}
