package fgo.cards.colorless;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.FifthFormPower;

public class FifthForm extends FGOCard {
    public static final String ID = makeID(FifthForm.class.getSimpleName());

    public FifthForm() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.RARE, CardColor.COLORLESS);
        setMagic(50, -15);
        portraitImg = ImageMaster.loadImage(cardPath("skill/FifthForm"));
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new FifthFormPower(p, 1, upgraded)));
    }
}


