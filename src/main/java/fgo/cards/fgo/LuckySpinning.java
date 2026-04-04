package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.StarRatePower;

public class LuckySpinning extends FGOCard {
    public static final String ID = makeID(LuckySpinning.class.getSimpleName());

    public LuckySpinning() {
        super(ID, 0, CardType.ATTACK, CardTarget.SELF, CardRarity.UNCOMMON);
        setStar(1, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new StarRatePower(p, star)));
    }
}


