package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.powers.BasePower;
import fgo.powers.CursePower;

public class CurseCondensingPower extends BasePower {
    public static final String POWER_ID = makeID(CurseCondensingPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public CurseCondensingPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.DEBUFF, false, owner, amount, "EndOfADreamPower");
    }

    @Override
    public void updateDescription() {
        this.description = this.amount == 1 ? DESCRIPTIONS[0] : String.format(DESCRIPTIONS[1], this.amount);
    }

    @Override
    public void atEndOfRound() {
        addToBot(new ReducePowerAction(owner, owner, this.ID, 1));
        if (this.amount == 1 && this.owner.hasPower(CursePower.POWER_ID)) {
            int CurAmt = this.owner.getPower(CursePower.POWER_ID).amount;
            addToBot(new RemoveSpecificPowerAction(owner, owner, this.ID));
            addToBot(new RemoveSpecificPowerAction(owner, owner, CursePower.POWER_ID));
            for (AbstractMonster mo : AbstractDungeon.getCurrRoom().monsters.monsters) {
                addToBot(new ApplyPowerAction(mo, owner, new CursePower(mo, owner, CurAmt), CurAmt));
            }
        }
    }
}
