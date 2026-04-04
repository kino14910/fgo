package fgo.powers;

import static fgo.FGOMod.makeID;

import java.util.Random;

import com.evacipated.cardcrawl.mod.stslib.powers.StunMonsterPower;
import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.NonStackablePower;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

public class TerrorPower extends BasePower implements NonStackablePower {
    public static final String POWER_ID = makeID(TerrorPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private final Random random = new Random();

    public TerrorPower(AbstractCreature owner, int turns, int chance) {
        super(POWER_ID, PowerType.DEBUFF, true, owner, turns, "EndOfADreamPower");
        amount2 = chance;
        updateDescription();
    }

    @Override
    public void updateDescription() {
        description = String.format(DESCRIPTIONS[0], amount2, amount); 
    }

    @Override
    public void atStartOfTurn() {
        flash();
        addToBot(new ReducePowerAction(owner, owner, this, 1));
        // amount%概率给眩晕
        if (random.nextInt(100) < amount2) {
            CardCrawlGame.sound.play("POWER_STUN");
            addToBot(new ApplyPowerAction(owner, owner, new StunMonsterPower((AbstractMonster) owner)));
            addToBot(new RemoveSpecificPowerAction(owner, owner, this));
        }
    }
}
