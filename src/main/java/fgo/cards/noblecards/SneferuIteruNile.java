package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo.DamageType;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.DeathChancePower;
import fgo.powers.WatersidePower;

public class SneferuIteruNile extends AbsNoblePhantasmCard {
    public static final String ID = makeID(SneferuIteruNile.class.getSimpleName());

    public SneferuIteruNile() {
        super(ID, AbstractCard.CardType.ATTACK, AbstractCard.CardTarget.ALL_ENEMY, 2);
        setDamage(35, 10);
        setMagic(10, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAllEnemiesAction(p, multiDamage, DamageType.NORMAL, AttackEffect.NONE));
        for (final AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo.isDead || mo.isDying) continue;
            addToBot(new ApplyPowerAction(mo, p, new VulnerablePower(mo, 3, false)));
            addToBot(new ApplyPowerAction(mo, p, new DeathChancePower(mo, magicNumber)));
        }
        addToBot(new ApplyPowerAction(p, p, new WatersidePower(p)));
    }
}

