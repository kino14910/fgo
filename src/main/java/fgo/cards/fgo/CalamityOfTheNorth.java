package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.PoisonPower;

import fgo.cards.FGOCard;
import fgo.powers.CursePower;

public class CalamityOfTheNorth extends FGOCard {
    public static final String ID = makeID(CalamityOfTheNorth.class.getSimpleName());

    public CalamityOfTheNorth() {
        super(ID, 1, CardType.SKILL, CardTarget.ALL_ENEMY, CardRarity.UNCOMMON);
        setMagic(3, 2);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            addToBot(new ApplyPowerAction(mo, p, new PoisonPower(mo, p, magicNumber)));
            addToBot(new ApplyPowerAction(mo, p, new CursePower(mo, p, magicNumber)));
        }
    }
}


