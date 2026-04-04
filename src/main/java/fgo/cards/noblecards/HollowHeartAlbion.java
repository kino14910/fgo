package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.StarPower;

public class HollowHeartAlbion extends AbsNoblePhantasmCard {
    public static final String ID = makeID(HollowHeartAlbion.class.getSimpleName());

    public HollowHeartAlbion() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(27, 8);
        setStar(10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAllEnemiesAction(p, multiDamage, DamageInfo.DamageType.HP_LOSS, AbstractGameAction.AttackEffect.FIRE));
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
    }

    @Override
    public AbstractCard makeCopy() {
        return new HollowHeartAlbion();
    }
}
