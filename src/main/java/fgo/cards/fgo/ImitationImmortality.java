package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;

import fgo.cards.FGOCard;
import fgo.powers.GutsPower;
import fgo.powers.NPRegenPower;
import fgo.powers.ReducePercentDamagePower;

public class ImitationImmortality extends FGOCard {
    public static final String ID = makeID(ImitationImmortality.class.getSimpleName());

    public ImitationImmortality() {
        super(ID, 2, CardType.POWER, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(3, 2);
        setCustomVar("metal", 3, 2);
        setCustomVar("damageReduction", 10, 10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new GutsPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, customVar("metal"))));
        addToBot(new ApplyPowerAction(p, p, new NPRegenPower(p, 10)));
        addToBot(new ApplyPowerAction(p, p, new ReducePercentDamagePower(p, customVar("damageReduction"))));
    }
}
