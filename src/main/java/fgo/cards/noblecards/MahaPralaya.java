package fgo.cards.noblecards;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.ShakeScreenAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.ScreenShake;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.BorderLongFlashEffect;
import com.megacrit.cardcrawl.vfx.combat.DieDieDieEffect;

import fgo.cards.AbsNoblePhantasmCard;

public class MahaPralaya extends AbsNoblePhantasmCard {
    public static final String ID = makeID(MahaPralaya.class.getSimpleName());

    public MahaPralaya() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(8, 3);
    }

    @Override
    public void calculateCardDamage(AbstractMonster m) {
        int MahaAmt = 0;
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            MahaAmt += mo.powers.size();
        }
        int realBaseDamage = baseDamage;
        baseDamage *= MahaAmt;
        super.calculateCardDamage(m);
        baseDamage = realBaseDamage;
        isDamageModified = damage != baseDamage;
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(new BorderLongFlashEffect(Color.LIGHT_GRAY)));
        addToBot(new VFXAction(new DieDieDieEffect(), 0.7F));
        addToBot(new ShakeScreenAction(0.0f, ScreenShake.ShakeDur.MED, ScreenShake.ShakeIntensity.HIGH));
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.SLASH_HORIZONTAL));
    }
}
