package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.FlightPower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.BlessingOfKurPower;

public class KurKigalIrkalla extends AbsNoblePhantasmCard {
    public static final String ID = makeID(KurKigalIrkalla.class.getSimpleName());

    public KurKigalIrkalla() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(26, 8);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (!mo.hasPower(FlightPower.POWER_ID)) {
                addToBot(new LoseHPAction(mo, p, mo.maxHealth / 10));
            }
        }
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.BLUNT_HEAVY));
    }

    @Override
    public void applyPowers() {
        AbstractPlayer p = AbstractDungeon.player;
        if (p.hasPower(BlessingOfKurPower.POWER_ID)) {
            rawDescription = cardStrings.DESCRIPTION
            + String.format(cardStrings.EXTENDED_DESCRIPTION[0], p.getPower(BlessingOfKurPower.POWER_ID).amount);
        }
        super.applyPowers();
        initializeDescription();
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = AbstractDungeon.player.hasPower(BlessingOfKurPower.POWER_ID)
                ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy()
                : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }
}
