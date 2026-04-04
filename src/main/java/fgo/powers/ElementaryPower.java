package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;

public class ElementaryPower extends BasePower {
    public static final String POWER_ID = makeID(ElementaryPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public ElementaryPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount);
    }

    @Override
    public void atStartOfTurn() {
        flash();
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo.currentBlock > 0) {
                addToBot(new LoseHPAction(mo, owner, mo.currentBlock, AbstractGameAction.AttackEffect.SHIELD));
                addToBot(new ApplyPowerAction(mo, owner, new VulnerablePower(mo, amount, false),
                        amount, AbstractGameAction.AttackEffect.NONE));
            }
        }
    }
}
