package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.NonStackableEnergyRegenPower;
import fgo.powers.WatersidePower;

public class LakeTexcoco extends FGOCard {
    public static final String ID = makeID(LakeTexcoco.class.getSimpleName());

    public LakeTexcoco() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setNP(20, 10);
        setMagic(5, 5);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new FgoNpAction(np));
        addToBot(new ApplyPowerAction(p, p, new NonStackableEnergyRegenPower(p, magicNumber, 3)));
        addToBot(new ApplyPowerAction(p, p, new WatersidePower(p)));
    }
}


