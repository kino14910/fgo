package fgo.relics;

import static fgo.FGOMod.makeID;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.characters.Master;

public class SuitcaseFgo extends BaseRelic {
    private static final String NAME = SuitcaseFgo.class.getSimpleName();
    public static final String ID = makeID(NAME);
    
    /**
     * {@link Master#preBattlePrep()}
     */
    public SuitcaseFgo() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.STARTER, LandingSound.MAGICAL);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    // @Override
    // public void receiveOnBattleStart(AbstractRoom abstractRoom) {
    //     addToBot(new FgoNpAction(20));
    //     flash();
    // }
}
