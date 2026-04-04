package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

public class BusterCardPower extends BasePower {
    public static final String POWER_ID = makeID(BusterCardPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public BusterCardPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount);
    }

    @Override
    public void updateDescription() {
        this.description = String.format(DESCRIPTIONS[0], this.amount, this.amount);
    }

    @Override
    public float atDamageFinalGive(float damage, DamageInfo.DamageType type, AbstractCard card) {
        if (card.type == AbstractCard.CardType.ATTACK) {
            return type == DamageInfo.DamageType.NORMAL ? damage + (float) this.amount : damage;
        }
        return damage;
    }

    @Override
    public float modifyBlock(float blockAmount, AbstractCard card) {
        if (card.type == AbstractCard.CardType.ATTACK) {
            return modifyBlock(blockAmount);
        }
        return blockAmount;
    }

    @Override
    public float modifyBlock(float blockAmount) {
        return blockAmount + (float) this.amount;
    }
}

