package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.WatersidePower;

public class GreatRamNautilus extends AbsNoblePhantasmCard {
    public static final String ID = makeID(GreatRamNautilus.class.getSimpleName());

    public GreatRamNautilus() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 2);
        setDamage(40, 12);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        if (p.hasPower(WatersidePower.POWER_ID)) {
            addToBot(new GainEnergyAction(1));
        }
    }

    @Override
    public void applyPowers() {
        int realBaseDamage = baseDamage;
        if (AbstractDungeon.player.hasPower(WatersidePower.POWER_ID)) {
            baseDamage += baseDamage / 2;
        }
        super.applyPowers();
        baseDamage = realBaseDamage;
        isDamageModified = damage != baseDamage;
    }

    @Override
    public void calculateCardDamage(AbstractMonster mo) {
        int realBaseDamage = baseDamage;
        baseDamage += magicNumber;
        if (AbstractDungeon.player.hasPower(WatersidePower.POWER_ID)) {
            baseDamage += baseDamage / 2;
        }
        super.calculateCardDamage(mo);
        baseDamage = realBaseDamage;
        isDamageModified = damage != baseDamage;
    }
}
