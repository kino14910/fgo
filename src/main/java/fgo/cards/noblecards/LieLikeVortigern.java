package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.IntangiblePlayerPower;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.AbsNoblePhantasmCard;

public class LieLikeVortigern extends AbsNoblePhantasmCard {
    public static final String ID = makeID(LieLikeVortigern.class.getSimpleName());
    
    public LieLikeVortigern() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 1);
        setDamage(25, 7);
        setMagic(1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.NONE));
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            addToBot(new ApplyPowerAction(mo, p, new StrengthPower(mo, -2)));
            addToBot(new ApplyPowerAction(mo, p, new IntangiblePlayerPower(mo, magicNumber)));
        }

        addToBot(new ApplyPowerAction(p, p, new IntangiblePlayerPower(p, magicNumber)));
    }
}
