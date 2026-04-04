package fgo.cards.fgo;

import com.evacipated.cardcrawl.mod.stslib.powers.StunMonsterPower;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.ViceCrushEffect;

import fgo.cards.FGOCard;
import fgo.powers.CursePower;

public class MysticEyes extends FGOCard {
    public static final String ID = makeID(MysticEyes.class.getSimpleName());

    public MysticEyes() {
        super(ID, 2, CardType.ATTACK, CardTarget.ENEMY, CardRarity.RARE);
        setDamage(10);
        setMagic(3, 3);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(new ViceCrushEffect(m.hb.cX, m.hb.cY), 0.5f));
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.NONE));
        addToBot(new ApplyPowerAction(m, p, new CursePower(m, p, magicNumber)));
        if (!m.hasPower(StunMonsterPower.POWER_ID)) {
            addToBot(new ApplyPowerAction(m, p, new StunMonsterPower(m)));
        }
    }
}


