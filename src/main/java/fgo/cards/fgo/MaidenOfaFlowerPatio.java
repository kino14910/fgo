package fgo.cards.fgo;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.CursePower;

public class MaidenOfaFlowerPatio extends FGOCard {
    public static final String ID = makeID(MaidenOfaFlowerPatio.class.getSimpleName());

    public MaidenOfaFlowerPatio() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setBlock(15, 5);
        setMagic(1);
        portraitImg = ImageMaster.loadImage(cardPath("skill/MaidenOfaFlowerPatio"));
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new GainBlockAction(p, p, block));
        addToBot(new ApplyPowerAction(p, p, new CursePower(p, p, magicNumber)));
    }
}


