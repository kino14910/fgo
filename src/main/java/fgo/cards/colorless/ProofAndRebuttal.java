package fgo.cards.colorless;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.actions.common.DrawCardAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.ProofAndRebuttalAction;
import fgo.cards.FGOCard;

public class ProofAndRebuttal extends FGOCard {
    public static final String ID = makeID(ProofAndRebuttal.class.getSimpleName());

    public ProofAndRebuttal() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.SPECIAL, CardColor.COLORLESS);
        setExhaust();
        portraitImg = ImageMaster.loadImage(cardPath("skill/ProofAndRebuttal"));
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DrawCardAction(p, 1));
        if (!AbstractDungeon.player.hand.isEmpty()) {
            addToBot(new ProofAndRebuttalAction());
        }
    }
}


