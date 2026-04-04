package fgo.relics;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.relics.AbstractRelic;

import fgo.characters.CustomEnums.FGOCardColor;

public class CosmicMedallion extends BaseRelic {
    private static final String NAME = CosmicMedallion.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    boolean isActive = false;
    boolean isChecking = false;
    
    public CosmicMedallion() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.SHOP, LandingSound.FLAT);
    }

    @Override
    public void atTurnStart() {
        isActive = true;
        isChecking = false;
        this.pulse = true;
        beginPulse();
    }
    
    @Override
    public void onPlayerEndTurn() {
        isChecking = true;
        this.pulse = false;
    }
    
    @Override
    public void onLoseHp(int damageAmount) {
        if (!isActive) {
            return;
        }
        if (isChecking && AbstractDungeon.actionManager.turnHasEnded) {
            isActive = false;
            this.pulse = false;
            return;
        } 
        
        flash();
        AbstractPlayer p = AbstractDungeon.player;
        addToTop(new GainBlockAction(p, p, damageAmount * 2));
        addToTop(new RelicAboveCreatureAction(p, this));
        
    }
    
    @Override
    public void onVictory() {
        this.pulse = false;
    }
    
    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public AbstractRelic makeCopy() {
        return new CosmicMedallion();
    }
}
