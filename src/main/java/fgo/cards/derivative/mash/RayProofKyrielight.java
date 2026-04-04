package fgo.cards.derivative.mash;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.AbstractPower;
import com.megacrit.cardcrawl.powers.VulnerablePower;

import fgo.action.IgnoresInvincibilityAction;
import fgo.cards.AbsNoblePhantasmCard;

public class RayProofKyrielight extends AbsNoblePhantasmCard {
    public static final String ID = makeID(RayProofKyrielight.class.getSimpleName());

    public RayProofKyrielight() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(30, 10);
        setMagic(3);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (AbstractMonster mo : AbstractDungeon.getCurrRoom().monsters.monsters) {
            addToBot(new IgnoresInvincibilityAction(mo));
        }

        addToBot(new AllEnemyApplyPowerAction(p, magicNumber, 
            monster -> new VulnerablePower(monster, magicNumber, false))
        );

        addToBot(new DamageAllEnemiesAction(p, multiDamage, DamageInfo.DamageType.NORMAL, AbstractGameAction.AttackEffect.SLASH_HEAVY));
        for (AbstractMonster mo : AbstractDungeon.getCurrRoom().monsters.monsters) {
            if (mo.type != AbstractMonster.EnemyType.BOSS) {
                for (AbstractPower pow : mo.powers) {
                    if (pow.type == AbstractPower.PowerType.BUFF) {
                        addToBot(new RemoveSpecificPowerAction(mo, p, pow));
                    }
                }
            }
        }
    }
}
