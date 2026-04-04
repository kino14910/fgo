package fgo.cards.fgo;

import static fgo.FGOMod.cardPath;

import com.badlogic.gdx.graphics.Color;
import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.evacipated.cardcrawl.mod.stslib.patches.FlavorText;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.WeakPower;

import fgo.cards.FGOCard;
import fgo.powers.BeyondTheFurthestEndPower;

public class BeyondTheFurthestEnd extends FGOCard {
    public static final String ID = makeID(BeyondTheFurthestEnd.class.getSimpleName());

    public BeyondTheFurthestEnd() {
        super(ID, 2, CardType.POWER, CardTarget.ALL_ENEMY, CardRarity.UNCOMMON);
        setMagic(3, 1);
        portraitImg = ImageMaster.loadImage(cardPath("power/BeyondTheFurthestEnd"));

        FlavorText.AbstractCardFlavorFields.textColor.set(this, Color.CHARTREUSE);
        FlavorText.AbstractCardFlavorFields.flavorBoxType.set(this, FlavorText.boxType.TRADITIONAL);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new AllEnemyApplyPowerAction(p, 3, 
            monster -> new WeakPower(monster, 3, false))
        );
        addToBot(new ApplyPowerAction(p, p, new BeyondTheFurthestEndPower(p, magicNumber)));
    }
}


