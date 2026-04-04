package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.action.FgoNpAction;

public class InsanityPower extends BasePower {
    public static final String POWER_ID = makeID(InsanityPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public InsanityPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "FightToDeathPower");
    }
    
    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        if (card.type == AbstractCard.CardType.ATTACK && owner.hasPower(CursePower.POWER_ID)) {
            flash();
            addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, 1)));
            addToBot(new FgoNpAction(amount));
            addToBot(new ReducePowerAction(owner, owner, CursePower.POWER_ID, 1));
        }
    }
}
