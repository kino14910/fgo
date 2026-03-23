package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.powers.BasePower;
import fgo.powers.CursePower;

public class PowerofGrudgePower extends BasePower {
    public static final String POWER_ID = makeID(PowerofGrudgePower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public PowerofGrudgePower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "AttackUpPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0];
    }

    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type) {
        return type == DamageInfo.DamageType.NORMAL && this.owner.hasPower(CursePower.POWER_ID) ? damage + (float)this.owner.getPower(CursePower.POWER_ID).amount : damage;
    }

    @Override
    public float atDamageReceive(float damage, DamageInfo.DamageType type) {
        if (type == DamageInfo.DamageType.NORMAL) {
            return this.owner != null && !this.owner.isPlayer && this.owner.hasPower(CursePower.POWER_ID) ? damage + (float)this.owner.getPower(CursePower.POWER_ID).amount : damage;
        }
        return damage;
    }
}
