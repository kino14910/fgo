package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;

import fgo.cards.FGOCard;
import fgo.powers.GutsPower;

public class LamplightOfTheSoul extends FGOCard {
    public static final String ID = makeID(LamplightOfTheSoul.class.getSimpleName());

    public LamplightOfTheSoul() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(5, 2);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GutsPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, magicNumber)));
    }
}