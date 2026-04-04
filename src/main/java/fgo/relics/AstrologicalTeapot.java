package fgo.relics;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.rooms.AbstractRoom;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.GutsPower;

public class AstrologicalTeapot extends BaseRelic {
    private static final String NAME = AstrologicalTeapot.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public AstrologicalTeapot() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.COMMON, LandingSound.FLAT);
    }

    @Override
    public void onUsePotion() {
        if (AbstractDungeon.getCurrRoom().phase == AbstractRoom.RoomPhase.COMBAT) {
            flash();
            AbstractPlayer p = AbstractDungeon.player;
            addToBot(new RelicAboveCreatureAction(p, this));
            addToBot(new ApplyPowerAction(p, p, new GutsPower(p, 2)));
        }
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }
}
