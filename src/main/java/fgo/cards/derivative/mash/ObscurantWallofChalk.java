package fgo.cards.derivative.mash;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.BufferPower;
import com.megacrit.cardcrawl.powers.IntangiblePlayerPower;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.powers.AntiPurgeDefensePower;

public class ObscurantWallofChalk extends FGOCard {
    public static final String ID = makeID(ObscurantWallofChalk.class.getSimpleName());

    public ObscurantWallofChalk() {
        super(ID, 2, CardType.POWER, CardTarget.SELF, CardRarity.SPECIAL, FGOCardColor.FGO_DERIVATIVE);
        setNP(20, 10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (!upgraded) {
            addToBot(new ApplyPowerAction(p, p, new BufferPower(p, 1)));
        } else {
            addToBot(new ApplyPowerAction(p, p, new IntangiblePlayerPower(p, 1)));
            addToBot(new ApplyPowerAction(p, p, new AntiPurgeDefensePower(p, 1)));
        }
        addToBot(new FgoNpAction(np));
    }
}
