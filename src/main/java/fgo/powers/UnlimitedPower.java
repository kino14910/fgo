package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.MakeTempCardInHandAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;

import basemod.cardmods.ExhaustMod;
import basemod.helpers.CardModifierManager;

public class UnlimitedPower extends BasePower {
    public static final String POWER_ID = makeID(UnlimitedPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public UnlimitedPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount);
    }

    @Override
    public void atStartOfTurn() {
        if (!AbstractDungeon.getMonsters().areMonstersBasicallyDead()) {
            for (int i = 0; i < amount; ++i) {
                AbstractCard card = AbstractDungeon.returnTrulyRandomCardInCombat(AbstractCard.CardType.ATTACK).makeCopy();
                card.setCostForTurn(0);
                CardModifierManager.addModifier(card, new ExhaustMod());
                addToBot(new MakeTempCardInHandAction(card, 1, true));
            }
        }
    }
}
