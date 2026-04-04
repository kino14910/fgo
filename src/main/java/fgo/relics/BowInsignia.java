package fgo.relics;

import static fgo.FGOMod.makeID;

import fgo.characters.CustomEnums.FGOCardColor;

public class BowInsignia extends BaseRelic {
    private static final String NAME = BowInsignia.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public BowInsignia() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.SPECIAL, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }
}
