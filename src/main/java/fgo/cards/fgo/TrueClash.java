package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class TrueClash extends FGOCard {
    public static final String ID = makeID(TrueClash.class.getSimpleName());

    public TrueClash() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(10, 3);
        setBlock(10, 3);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.SLASH_HEAVY));
        addToBot(new GainBlockAction(p, block));
    }

    @Override
    public boolean canUse(AbstractPlayer p, AbstractMonster m) {
        boolean canUse = super.canUse(p, m);
        if (!canUse) {
            return false;
        }
        
        if (m != null && m.getIntentBaseDmg() <= 0) {
            canUse = false;
            cantUseMessage = cardStrings.EXTENDED_DESCRIPTION[0];
        }

        return canUse;
    }

    @Override
    public void triggerOnGlowCheck() {
        for (AbstractMonster m : AbstractDungeon.getMonsters().monsters) {
            if (m != null && m.getIntentBaseDmg() > 0) {
                glowColor = AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
                break;
            }
        }
    }
}


