package fgo.powers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.actions.tempHp.AddTemporaryHPAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.noblecards.KurKigalIrkalla;

public class BlessingOfKurPower extends BasePower {
    public static final String POWER_ID = makeID(BlessingOfKurPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    private final int maxHP;
    private final int strength;

    public BlessingOfKurPower(AbstractCreature owner, int maxHP, int strength) {
        super(POWER_ID, PowerType.BUFF, false, owner);
        this.maxHP = maxHP;
        this.strength = strength;
        updateDescription();
    }

    @Override
    public void updateDescription() {description = String.format(DESCRIPTIONS[0], maxHP, strength);}

    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        if (card.cardID.equals(KurKigalIrkalla.ID)) {
            addToBot(new AddTemporaryHPAction(owner, owner, maxHP));
            addToBot(new ApplyPowerAction(owner, owner, new StrengthPower(owner, strength)));
            addToBot(new RemoveSpecificPowerAction(owner, owner, ID));
        }
    }

    
}
