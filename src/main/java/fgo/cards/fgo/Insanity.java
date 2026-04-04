package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.CursePower;
import fgo.powers.InsanityPower;

public class Insanity extends FGOCard {
    public static final String ID = makeID(Insanity.class.getSimpleName());

    public Insanity() {
        super(ID, 1, CardType.POWER, CardTarget.ALL_ENEMY, CardRarity.RARE);
        setMagic(10, 10);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new InsanityPower(p, magicNumber)));
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo.hasPower(CursePower.POWER_ID)) {
                int CurseAmt = mo.getPower(CursePower.POWER_ID).amount;
                addToBot(new ApplyPowerAction(p, p, new CursePower(p, p, CurseAmt)));
                addToBot(new RemoveSpecificPowerAction(mo, p, CursePower.POWER_ID));
            }
        }

    }
}


