package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.ArtifactPower;

import fgo.cards.FGOCard;
import fgo.powers.GutsPower;

public class MaidensWillpower extends FGOCard {
    public static final String ID = makeID(MaidensWillpower.class.getSimpleName());

    public MaidensWillpower() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setCostUpgrade(0);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GutsPower(p, 5)));
        addToBot(new ApplyPowerAction(p, p, new ArtifactPower(p, 1)));
    }
}


