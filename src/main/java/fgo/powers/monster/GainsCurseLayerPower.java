package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.powers.BasePower;
import fgo.powers.CursePower;

public class GainsCurseLayerPower extends BasePower {
    public static final String POWER_ID = makeID(GainsCurseLayerPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public GainsCurseLayerPower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "BuffRegenPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0];
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        flash();
        addToBot(new ApplyPowerAction(owner, owner, new CursePower(owner, owner, 3), 3, AbstractGameAction.AttackEffect.NONE));
    }
}
