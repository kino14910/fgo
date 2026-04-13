package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.MetallicizePower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.NPRegenPower;
import fgo.powers.StarRegenPower;

public class GardenOfAvalon extends AbsNoblePhantasmCard {
    public static final String ID = makeID(GardenOfAvalon.class.getSimpleName());

    public GardenOfAvalon() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setBlock(3, 1);
        setNP(5);
        setStar(5, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new MetallicizePower(p, block)));
        addToBot(new ApplyPowerAction(p, p, new NPRegenPower(p, np)));
        addToBot(new ApplyPowerAction(p, p, new StarRegenPower(p, star)));
    }
}
