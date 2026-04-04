package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.unique.BaneAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;

public class YewBow extends AbsNoblePhantasmCard {
    public static final String ID = makeID(YewBow.class.getSimpleName());

    public YewBow() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 1);
        setDamage(25, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.SLASH_HORIZONTAL));
        addToBot(new BaneAction(m, new DamageInfo(p, damage, damageTypeForTurn)));
    }
}
