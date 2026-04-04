package fgo.relics;

import static fgo.FGOMod.makeID;

import fgo.characters.CustomEnums.FGOCardColor;

public class MidsummerNightDream extends BaseRelic {
    private static final String NAME = MidsummerNightDream.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public MidsummerNightDream() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.UNCOMMON, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }
}
