package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.DrawCardAction;
import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.cards.FGOCard;
import fgo.utils.ModHelper;

public class GodsExecution extends FGOCard {
    public static final String ID = makeID(GodsExecution.class.getSimpleName());

    public GodsExecution() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(5, 3);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        ModHelper.addToBotAbstract(() -> {
            if (ModHelper.hasBuff(p, AbstractPower.PowerType.DEBUFF)) {
                addToTop(new DrawCardAction(p, 1));
                addToTop(new GainEnergyAction(1));
            }
            addToTop(new DamageAction(m, new DamageInfo(p, this.damage, this.damageTypeForTurn), AttackEffect.BLUNT_HEAVY));
        });
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = ModHelper.hasBuff(AbstractDungeon.player, AbstractPower.PowerType.DEBUFF)
            ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy()
            : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }
}


