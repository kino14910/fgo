package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.CursePower;
import fgo.powers.GutsPower;

public class VoidSpaceFineArts extends FGOCard {
    public static final String ID = makeID(VoidSpaceFineArts.class.getSimpleName());

    public VoidSpaceFineArts() {
        super(ID, 1, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(5, 5);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void upgrade() {
        if (!upgraded) {
            upgradeName();
            upgradeMagicNumber(5);
        }
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GutsPower(p, 10)));
        for (int i = 0; i < 3; ++i) {
            addToBot(new ApplyPowerAction(p, p, new CursePower(p, p, 1)));
        }
        
        if (p.hasPower(CursePower.POWER_ID)) {
            int curseAmt = p.getPower(CursePower.POWER_ID).amount;
            addToBot(new FgoNpAction(magicNumber * curseAmt));
        }
    }
}


