package fgo.powers.monster;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.powers.BasePower;

public class CooldownFgoPower extends BasePower {
    public static final String POWER_ID = makeID(CooldownFgoPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    private final AbstractCard card;

    public CooldownFgoPower(AbstractCreature owner, int cardAmt, AbstractCard copyMe) {
        super(POWER_ID + copyMe.makeStatEquivalentCopy().cardID, PowerType.BUFF, false, owner, null, cardAmt, null, false);
        this.card = copyMe.makeStatEquivalentCopy();
        this.card.resetAttributes();
        this.updateDescription();
    }

    @Override
    public void updateDescription() {
        // this.description = DESCRIPTIONS[0] + FontHelper.colorString(this.card.name, "y") + DESCRIPTIONS[1] + this.amount + DESCRIPTIONS[2];
        this.description = String.format(DESCRIPTIONS[0], FontHelper.colorString(this.card.name, "y"), this.amount);
    }

    @Override
    public void atStartOfTurnPostDraw() {
        if (!AbstractDungeon.getMonsters().areMonstersBasicallyDead()) {
            addToBot(new ReducePowerAction(owner, owner, this.ID, 1));
        }
    }
}
