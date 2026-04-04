package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.StarPower;

public class Swingby extends FGOCard {
    public static final String ID = makeID(Swingby.class.getSimpleName());

    public Swingby() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.COMMON);
        setBlock(12, 6);
        setStar(4, 2);
        tags.add(FGO_Foreigner);
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new GainBlockAction(p, block));
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
    }
}


