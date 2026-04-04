package fgo.relics;

import static fgo.FGOMod.makeID;
import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RelicAboveCreatureAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.PowerTip;
import com.megacrit.cardcrawl.powers.watcher.VigorPower;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.StarPower;

public class Salem extends BaseRelic {
    private static final String NAME = Salem.class.getSimpleName();
    public static final String ID = makeID(NAME);
    
    public Salem() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.UNCOMMON, LandingSound.FLAT);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    private int countFGO_ForeignerCards() {
        int count = 0;
        for (AbstractCard c : AbstractDungeon.player.masterDeck.group) {
            if (c.hasTag(FGO_Foreigner)) {
                count++;
            }
        }
        return count;
    }

    private void updateDescriptionAndTips() {
        description = DESCRIPTIONS[0] + (counter == 0 
            ? String.format(DESCRIPTIONS[1])
            : String.format(DESCRIPTIONS[2], counter));

        tips.clear();
        tips.add(new PowerTip(name, description));
        initializeTips();
    }

    @Override
    public void setCounter(int c) {
        counter = c;
        updateDescriptionAndTips();
    }

    @Override
    public void onMasterDeckChange() {
        counter = countFGO_ForeignerCards();
        updateDescriptionAndTips();
    }

    @Override
    public void onEquip() {
        counter = countFGO_ForeignerCards();
        updateDescriptionAndTips();
    }

    @Override
    public void atTurnStart() {
        AbstractPlayer p = AbstractDungeon.player;
        if (counter > 0) {
            flash();
            addToTop(new ApplyPowerAction(p, p, new VigorPower(p, counter * 2)));
            addToTop(new ApplyPowerAction(p, p, new StarPower(p, counter * 2)));
            addToTop(new RelicAboveCreatureAction(p, this));
        }
    }
}
