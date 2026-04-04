package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.WaitAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.FlameBarrierEffect;
import com.megacrit.cardcrawl.vfx.combat.WeightyImpactEffect;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.StarPower;

public class LastSunXibalba extends AbsNoblePhantasmCard {
    public static final String ID = makeID(LastSunXibalba.class.getSimpleName());
    
    public LastSunXibalba() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(6, 2);
        setStar(30);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo != null) {
                addToBot(new VFXAction(new WeightyImpactEffect(mo.hb.cX, mo.hb.cY)));
            }
            addToBot(new WaitAction(0.8F));
        }

        for (int i = 0; i < 5; ++i) {
            addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        }
        
        addToBot(new VFXAction(p, new FlameBarrierEffect(p.hb.cX, p.hb.cY), Settings.FAST_MODE ? 0.1f : 0.5f));
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
    }
}
