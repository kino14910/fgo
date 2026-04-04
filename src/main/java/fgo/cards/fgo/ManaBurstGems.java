package fgo.cards.fgo;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.NoDrawPower;
import com.megacrit.cardcrawl.vfx.combat.VerticalAuraEffect;

import fgo.cards.FGOCard;
import fgo.powers.ManaBurstGemsPower;

public class ManaBurstGems extends FGOCard {
    public static final String ID = makeID(ManaBurstGems.class.getSimpleName());

    public ManaBurstGems() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(3, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new ManaBurstGemsPower(p, magicNumber)));
        addToBot(new VFXAction(p, new VerticalAuraEffect(Color.FIREBRICK, p.hb.cX, p.hb.cY), 0.0f));
        addToBot(new ApplyPowerAction(p, p, new NoDrawPower(p)));
    }
}


