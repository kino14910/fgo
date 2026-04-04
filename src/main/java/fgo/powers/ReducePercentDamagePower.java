package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class ReducePercentDamagePower extends BasePower {
    public static final String POWER_ID = makeID(ReducePercentDamagePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    
    public ReducePercentDamagePower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, Math.min(amount, 100), "DefenseUpPower");
    }

    @Override
    public float atDamageFinalReceive(float damage, DamageInfo.DamageType type) {
        return damage * (100.0f - amount) / 100.0f;
    }

    @Override
    public void atStartOfTurn() {
        if (!AbstractDungeon.getMonsters().areMonstersBasicallyDead()) {
            addToBot(new RemoveSpecificPowerAction(owner, owner, ID));
        }
    }
}
