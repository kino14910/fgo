package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.powers.BasePower;
import fgo.powers.CursePower;

public class TaintedCursePower extends BasePower {
    public static final String POWER_ID = makeID(TaintedCursePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public TaintedCursePower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "DefenseUpPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0];
    }

    @Override
    public int onAttacked(DamageInfo info, int damageAmount) {
        if (info.type != DamageInfo.DamageType.THORNS && info.type != DamageInfo.DamageType.HP_LOSS && info.owner != null && info.owner != this.owner) {
            flash();
            addToBot(new ApplyPowerAction(info.owner, owner, new CursePower(info.owner, owner, 1), 1, AbstractGameAction.AttackEffect.NONE));
        }
        return damageAmount;
    }

    @Override
    public float atDamageReceive(float damage, DamageInfo.DamageType type) {
        return type != DamageInfo.DamageType.HP_LOSS && type != DamageInfo.DamageType.THORNS ? damage / 2.0f : damage;
    }
}
