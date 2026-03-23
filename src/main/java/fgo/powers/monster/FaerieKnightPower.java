package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.ArtifactPower;

import fgo.cards.derivative.mash.Camelot;
import fgo.powers.BasePower;

public class FaerieKnightPower extends BasePower {
    public static final String POWER_ID = makeID(FaerieKnightPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public FaerieKnightPower(AbstractCreature owner) {
        super(POWER_ID, PowerType.BUFF, false, owner, "DefenseUpPower");
    }

    @Override
    public void updateDescription() {
        this.description = DESCRIPTIONS[0];
    }

    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        if (card.cardID.equals(Camelot.ID)) {
            flash();
            addToBot(new ApplyPowerAction(owner, owner, new ArtifactPower(owner, 5), 5));
            addToBot(new RemoveSpecificPowerAction(owner, owner, this.ID));
        }
    }
}
