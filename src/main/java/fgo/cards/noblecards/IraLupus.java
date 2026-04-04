package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.vfx.combat.CleaveEffect;

import fgo.cards.AbsNoblePhantasmCard;

public class IraLupus extends AbsNoblePhantasmCard {
    public static final String ID = makeID(IraLupus.class.getSimpleName());

    public IraLupus() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 1);
        setDamage(24, 10);
        setMagic(2, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction("ATTACK_HEAVY"));
        addToBot(new VFXAction(p, new CleaveEffect(), 0.1F));
        addToBot(new DamageAction(m, new DamageInfo(m, damage, DamageInfo.DamageType.HP_LOSS), AbstractGameAction.AttackEffect.FIRE));
        addToBot(new ApplyPowerAction(m, p, new VulnerablePower(m, magicNumber, false), magicNumber, true, AbstractGameAction.AttackEffect.NONE));
    }
}
