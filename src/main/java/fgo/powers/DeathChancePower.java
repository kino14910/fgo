package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.BetterOnApplyPowerPower;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.actions.watcher.JudgementAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.powers.interfaces.onEnemyLoseHpPower;

public class DeathChancePower extends BasePower implements BetterOnApplyPowerPower, onEnemyLoseHpPower {
    public static final String POWER_ID = makeID(DeathChancePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public DeathChancePower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.DEBUFF, false, owner, amount); 
    }

    @Override
    public boolean betterOnApplyPower(AbstractPower power, AbstractCreature target, AbstractCreature source) {
        if (power == this) return true;
        if (power.ID == DeathChancePower.POWER_ID && target.currentHealth <= amount) {
            addToBot(new JudgementAction(target, amount));
            addToBot(new RemoveSpecificPowerAction(target, source, power));
        };
        return true;
    }

    @Override
    public void onEnemyLoseHp(DamageInfo info) {
        if (owner.currentHealth <= amount) {
            addToBot(new JudgementAction(owner, amount));
        }
    }
}
