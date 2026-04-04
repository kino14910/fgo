package fgo.cards.fgo;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.action.SparksRouteAction;
import fgo.cards.FGOCard;
import fgo.characters.Master;

public class SparksRoute extends FGOCard {
    public static final String ID = makeID(SparksRoute.class.getSimpleName());

    public SparksRoute() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setExhaust(true, false);
        portraitImg = ImageMaster.loadImage(cardPath("skill/SparksRoute"));
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(-10));
        addToBot(new SparksRouteAction(this.upgraded));
    }

    @Override
    public boolean canUse(AbstractPlayer p, AbstractMonster m) {
        boolean canUse = super.canUse(p, m);
        if (!canUse) {
            return false;
        }

        if (Master.fgoNp < 10) {
            canUse = false;
            cantUseMessage = cardStrings.EXTENDED_DESCRIPTION[0];
        }

        return canUse;
    }

    @Override
    public void triggerOnGlowCheck() {
        if (Master.fgoNp >= 10) {
            glowColor = AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
        }
    }
}


