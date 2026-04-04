package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.InvinciblePower;
import com.megacrit.cardcrawl.vfx.combat.MindblastEffect;

import fgo.action.IgnoresInvincibilityAction;
import fgo.cards.AbsNoblePhantasmCard;

public class LostLonginus extends AbsNoblePhantasmCard {
    public static final String ID = makeID(LostLonginus.class.getSimpleName());
    
    public LostLonginus() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(24, 6);
        setMagic(100, 100);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new IgnoresInvincibilityAction(m));
        addToBot(new SFXAction("ATTACK_HEAVY"));
        addToBot(new VFXAction(p, new MindblastEffect(p.dialogX, p.dialogY, p.flipHorizontal), 0.1F));
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.NONE));

        for (AbstractMonster m2 : AbstractDungeon.getMonsters().monsters) {
            if (!m2.isDeadOrEscaped()) {
                if (m2.hasPower(InvinciblePower.POWER_ID)) {
                    addToBot(new ApplyPowerAction(m2, p, new InvinciblePower(m2, magicNumber)));
                }
            }
        }
    }
}
