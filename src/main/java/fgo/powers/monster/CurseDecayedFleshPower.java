package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.VulnerablePower;

import fgo.powers.BasePower;
import fgo.powers.CursePower;

public class CurseDecayedFleshPower extends BasePower {
    public static final String POWER_ID = makeID(CurseDecayedFleshPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public CurseDecayedFleshPower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "InfiniteGrowthPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0] + DESCRIPTIONS[1];
    }

    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type) {
        if (type == DamageInfo.DamageType.NORMAL && this.owner.hasPower(CursePower.POWER_ID)) {
            int curseAmount = this.owner.getPower(CursePower.POWER_ID).amount;
            float damageBonus = damage * ((float)curseAmount * 0.1f);
            return damage + damageBonus;
        }
        return damage;
    }

    @Override
    public float atDamageReceive(float damage, DamageInfo.DamageType type) {
        if (type == DamageInfo.DamageType.NORMAL && this.owner != null && !this.owner.isPlayer && this.owner.hasPower(CursePower.POWER_ID)) {
            int curseAmount = this.owner.getPower(CursePower.POWER_ID).amount;
            float damageBonus = damage * ((float)curseAmount * 0.1f);
            return damage - damageBonus;
        }
        return damage;
    }

    @Override
    public int onAttacked(DamageInfo info, int damageAmount) {
        if (info.type != DamageInfo.DamageType.THORNS && info.type != DamageInfo.DamageType.HP_LOSS && info.owner != null && info.owner != this.owner && this.owner.hasPower(CursePower.POWER_ID)) {
            flash();
            addToBot(new ReducePowerAction(owner, owner, CursePower.POWER_ID, 1));
            addToBot(new HealAction(owner, owner, 10));
            addToBot(new ApplyPowerAction(info.owner, owner, new CursePower(info.owner, owner, 1), 1, AbstractGameAction.AttackEffect.NONE));
            addToBot(new ApplyPowerAction(info.owner, owner, new VulnerablePower(info.owner, 1, false), 1, AbstractGameAction.AttackEffect.NONE));
        }
        return damageAmount;
    }
}
