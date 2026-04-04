package fgo.potions;

import static fgo.FGOMod.makeID;

import java.util.ArrayList;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.watcher.ChooseOneAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.helpers.PowerTip;
import com.megacrit.cardcrawl.helpers.TipHelper;
import com.megacrit.cardcrawl.helpers.input.InputHelper;
import com.megacrit.cardcrawl.localization.PotionStrings;

import basemod.BaseMod;
import fgo.cards.optionCards.Kaleidoscope;
import fgo.cards.optionCards.TheBlackGrail;

public class ExtremelySpicyMapoTofu extends BasePotion {
    public static final String ID = makeID(ExtremelySpicyMapoTofu.class.getSimpleName());
    private static final PotionStrings potionStrings = CardCrawlGame.languagePack.getPotionString(ID);
    public static final Color NOBLE = CardHelper.getColor(255, 215, 0);
    
    public ExtremelySpicyMapoTofu() {
        super(ID, 0, PotionRarity.RARE, PotionSize.BOTTLE, Color.ORANGE, Color.RED, null);
        labOutlineColor = NOBLE;
        isThrown = false;
    }

    @Override
    public void addAdditionalTips() {
        tips.add(new PowerTip(
                TipHelper.capitalize(BaseMod.getKeywordTitle("fgo:np")),
                BaseMod.getKeywordDescription("fgo:np")));
        tips.add(new PowerTip(
                TipHelper.capitalize(BaseMod.getKeywordTitle("fgo:np_damage")),
                BaseMod.getKeywordDescription("fgo:np_damage")));
    }

    @Override
    public void use(AbstractCreature target) {
        InputHelper.moveCursorToNeutralPosition();
        ArrayList<AbstractCard> stanceChoices = new ArrayList<>();
        stanceChoices.add(new Kaleidoscope());
        stanceChoices.add(new TheBlackGrail());
        addToBot(new ChooseOneAction(stanceChoices));
    }
    
    @Override
    public String getDescription() {
        return potionStrings.DESCRIPTIONS[0];
    }
}
