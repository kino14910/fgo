package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import basemod.ReflectionHacks;
import fgo.cards.FGOCard;
import fgo.powers.ReducePercentDamagePower;

public class HolyShroud extends FGOCard {
    public static final String ID = makeID(HolyShroud.class.getSimpleName());

    public HolyShroud() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(25, 5);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        int sum = 0;
        
        for (AbstractMonster monster : AbstractDungeon.getMonsters().monsters) {
            int dmg = (int) ReflectionHacks.getPrivate(monster, AbstractMonster.class, "intentDmg");
            int actualDmg = (boolean) ReflectionHacks.getPrivate(monster, AbstractMonster.class, "isMultiDmg")
                    ? dmg * (int) ReflectionHacks.getPrivate(monster, AbstractMonster.class, "intentMultiAmt")
                    : dmg;
            sum += actualDmg;
        }
        
        if (sum >= 20) {
            addToBot(new ApplyPowerAction(p, p, new ReducePercentDamagePower(p, magicNumber)));
        }

    }
}


