package fgo.cards.noblecards;

import static fgo.utils.ModHelper.addToBotAbstract;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.ShakeScreenAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.ScreenShake;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.LoseStrengthPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.BorderLongFlashEffect;
import com.megacrit.cardcrawl.vfx.combat.DieDieDieEffect;

import basemod.ReflectionHacks;
import fgo.action.FgoNpAction;
import fgo.cards.AbsNoblePhantasmCard;

public class Sevendrive extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Sevendrive.class.getSimpleName());

    public Sevendrive() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(24, 8);
        setNP(10, 20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(new BorderLongFlashEffect(Color.LIGHT_GRAY)));
        addToBot(new VFXAction(new DieDieDieEffect(), 0.7F));
        addToBot(new ShakeScreenAction(0.0f, ScreenShake.ShakeDur.MED, ScreenShake.ShakeIntensity.HIGH));
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 2)));
        addToBot(new ApplyPowerAction(p, p, new LoseStrengthPower(p, 2)));
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.SLASH_HORIZONTAL, true));
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo == null || mo.isDeadOrEscaped()) 
                return;
            int intentMultiAmt = (int) ReflectionHacks.getPrivate(mo, AbstractMonster.class, "intentMultiAmt");
            if (intentMultiAmt > 1) {
                addToBotAbstract(() -> {
                    ReflectionHacks.setPrivate(mo, AbstractMonster.class, "intentMultiAmt", intentMultiAmt - 1);
                });
            }
        }
        addToBot(new FgoNpAction(np));
    }
}
