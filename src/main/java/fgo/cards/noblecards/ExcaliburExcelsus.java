package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.action.FgoNpAction;
import fgo.action.IgnoresInvincibilityAction;
import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.NPOverChargePower;

public class ExcaliburExcelsus extends AbsNoblePhantasmCard {
    public static final String ID = makeID(ExcaliburExcelsus.class.getSimpleName());

    public ExcaliburExcelsus() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 2);
        setDamage(16, 6);
        setBlock(6);
        setMagic(2);
        setNP(20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new IgnoresInvincibilityAction(m));

        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (!mo.isDeadOrEscaped()) {
                addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, magicNumber)));
                addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(1)));
                addToBot(new GainBlockAction(p, block));
                addToBot(new FgoNpAction(np));
            }
        }

        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.NONE));
    }
}