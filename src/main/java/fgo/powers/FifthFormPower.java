package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.GameActionManager;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.AbstractCard.CardTarget;
import com.megacrit.cardcrawl.cards.AbstractCard.CardType;
import com.megacrit.cardcrawl.cards.DamageInfo.DamageType;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

public class FifthFormPower extends BasePower {
    public static final String POWER_ID = makeID(FifthFormPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public FifthFormPower(AbstractCreature owner, int amount, boolean upgraded) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount); 
        this.amount2 = upgraded ? 35 : 50;
        updateDescription();
    }

    @Override
    public void updateDescription() {
        description = amount == 1 
            ? String.format(DESCRIPTIONS[0], amount2) 
            : String.format(DESCRIPTIONS[1], amount, amount2);
    }

    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        if (card.purgeOnUse || card.type != CardType.ATTACK || (card.target != CardTarget.ENEMY && card.target != CardTarget.SELF_AND_ENEMY) || action.target == null) {
            return;
        }
        flash();
        AbstractMonster m = (AbstractMonster) action.target;
        GameActionManager.queueExtraCard(card, m);
    }

    @Override
    public float atDamageGive(float damage, DamageType type, AbstractCard card) {
        if ((card.target == CardTarget.ENEMY || card.target == CardTarget.SELF_AND_ENEMY) && card.type == CardType.ATTACK) {
            return type != DamageType.THORNS ? damage * (100 - amount2) / 100.0f : damage;
        }

        return atDamageGive(damage, type);
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        addToBot(new ReducePowerAction(owner, owner, this, 1));
    }
}
