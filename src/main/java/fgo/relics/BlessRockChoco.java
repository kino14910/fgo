package fgo.relics;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;

import fgo.characters.CustomEnums.FGOCardColor;

public class BlessRockChoco extends BaseRelic {
    private static final String NAME = BlessRockChoco.class.getSimpleName();
	public static final String ID = makeID(NAME);
    
    public BlessRockChoco() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.UNCOMMON, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public void onCardDraw(AbstractCard drawnCard) {
        if (drawnCard.costForTurn >= 3) {
            flash();
            addToBot(new RelicAboveCreatureAction(AbstractDungeon.player, this));
            drawnCard.costForTurn -= 1;
            drawnCard.isCostModifiedForTurn = true;
        }
    }
}
