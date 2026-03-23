package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.powers.ThornsPower;

import fgo.powers.BasePower;

public class NowStormPower extends BasePower {
    public static final String POWER_ID = makeID(NowStormPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public NowStormPower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "BuffRegenPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0];
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        flash();
        addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, 1), 1));
        addToBot(new ApplyPowerAction(owner, owner, new ThornsPower(owner, 1), 1));
    }
}
