package fgo.relics;

import static fgo.FGOMod.makeID;

import com.badlogic.gdx.math.MathUtils;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.powers.DexterityPower;
import com.megacrit.cardcrawl.powers.FrailPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.powers.WeakPower;
import com.megacrit.cardcrawl.rooms.AbstractRoom;

import fgo.characters.CustomEnums.FGOCardColor;

public class BB extends BaseRelic {
    private static final String NAME = BB.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public BB() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.SPECIAL, LandingSound.FLAT);
    }

    @Override
    public void atBattleStart() {
        flash();
        grayscale = true;
        AbstractPlayer p = AbstractDungeon.player;
        addToBot(new RelicAboveCreatureAction(p, this));
        int roll = MathUtils.random(1);

        if (roll == 0) {
            addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 2)));
            addToBot(new ApplyPowerAction(p, p, new DexterityPower(p, 2)));
            return;
        }

        int roll_debuff = MathUtils.random(2);
        switch (roll_debuff) {
            case 0:
                addToBot(new ApplyPowerAction(p, p, new VulnerablePower(p, 1, true)));
                break;
            case 1:
                addToBot(new ApplyPowerAction(p, p, new WeakPower(p, 1, true)));
                break;
            default:
                addToBot(new ApplyPowerAction(p, p, new FrailPower(p, 1, true)));
                break;
        }
    }

    @Override
    public void justEnteredRoom(AbstractRoom room) {
        grayscale = false;
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }
}
