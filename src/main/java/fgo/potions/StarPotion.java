package fgo.potions;

import static fgo.FGOMod.makeID;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.helpers.PowerTip;
import com.megacrit.cardcrawl.helpers.TipHelper;

import basemod.BaseMod;
import fgo.powers.StarPower;

public class StarPotion extends BasePotion {
    public static final String ID = makeID(StarPotion.class.getSimpleName());
    public static final Color NOBLE = CardHelper.getColor(255, 215, 0);
    
    public StarPotion() {
        super(ID, 10, PotionRarity.COMMON, PotionSize.MOON, NOBLE, NOBLE, null);
        labOutlineColor = NOBLE;
        isThrown = false;
    }

    @Override
    public void addAdditionalTips() {
        tips.add(new PowerTip(
            TipHelper.capitalize(BaseMod.getKeywordTitle(makeID("star"))),
            BaseMod.getKeywordDescription(makeID("star"))
        ));
    }

    @Override
    public void use(AbstractCreature target) {
        addToBot(new ApplyPowerAction(AbstractDungeon.player, AbstractDungeon.player, new StarPower(AbstractDungeon.player, potency), potency));
    }
    
    @Override
    public String getDescription() {
        return String.format(DESCRIPTIONS[0], potency);
    }
}
