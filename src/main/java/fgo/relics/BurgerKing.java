package fgo.relics;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import fgo.cards.colorless.Dumuzid;
import fgo.characters.CustomEnums.FGOCardColor;

public class BurgerKing extends BaseRelic {
    private static final String NAME = BurgerKing.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public BurgerKing() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.BOSS, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public void onEquip() {
        AbstractDungeon.player.increaseMaxHp(40, true);
        for (int i = 0; i < 3; i++) {
            AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new Dumuzid(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
        }
    }
}
