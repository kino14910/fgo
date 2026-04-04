package fgo.cards.derivative.mash;

import static fgo.utils.ModHelper.getPowerCount;

import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.PurgeField;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInDiscardAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.NPRatePower;
import fgo.powers.StarPower;

public class TimewornBulletKindling extends FGOCard {
    public static final String ID = makeID(TimewornBulletKindling.class.getSimpleName());

    public TimewornBulletKindling() {
        super(ID, 0, CardType.POWER, CardTarget.SELF, CardRarity.SPECIAL, FGOCardColor.FGO_DERIVATIVE);
        setMagic(20);
        PurgeField.purge.set(this, true);
        cardsToPreview = new ObscurantWallofChalk();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, magicNumber)));
        int starAmount = getPowerCount(p, StarPower.POWER_ID);
        if (starAmount > 0) {
            addToBot(new RemoveSpecificPowerAction(p, p, StarPower.POWER_ID));
            addToBot(new FgoNpAction(starAmount * 4));
        }
        addToBot(new LoseHPAction(p, p, 4));
        
        FGOCard obscurantWallofChalk = new ObscurantWallofChalk();
        for (AbstractCard c : AbstractDungeon.player.masterDeck.group) {
            if (c.cardID == ObscurantWallofChalk.ID && c.upgraded) {
                obscurantWallofChalk.upgrade();
                break;
            }
        }
        addToBot(new MakeTempCardInDiscardAction(obscurantWallofChalk, true));
    }
}
