package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;

public class BeyondTheFurthestEndPower extends BasePower {
    public static final String POWER_ID = makeID(BeyondTheFurthestEndPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public BeyondTheFurthestEndPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "BuffRegenPower");
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        if (!AbstractDungeon.getMonsters().areMonstersBasicallyDead() && isPlayer) {
            flash();
            addToBot(new AllEnemyApplyPowerAction(owner, -1, 
                monster -> new StrengthPower(monster, -1)));
        }
        addToBot(new ReducePowerAction(owner, owner, this, 1));
    }
 
   @Override
    public void updateDescription() {
        description = DESCRIPTIONS[0];
    }
}
