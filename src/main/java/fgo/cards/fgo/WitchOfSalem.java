package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.powers.WeakPower;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.powers.TerrorPower;
import fgo.powers.VSTerrorDamageUpPower;

public class WitchOfSalem extends FGOCard {
    public static final String ID = makeID(WitchOfSalem.class.getSimpleName());

    public WitchOfSalem() {
        super(ID, 3, CardType.SKILL, CardTarget.ALL_ENEMY, CardRarity.RARE);
        setMagic(30, 20);
        setCustomVar("terror", 50, 50);

        setNP(20);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (mo.isDead || mo.isDying) continue;
            addToBot(new ApplyPowerAction(mo, p, new VulnerablePower(mo, 3, false)));
            addToBot(new ApplyPowerAction(mo, p, new WeakPower(mo, 3, false)));
            addToBot(new ApplyPowerAction(mo, p, new TerrorPower(mo, 3, magicNumber)));
        }
        
        addToBot(new ApplyPowerAction(p, p, new VSTerrorDamageUpPower(p, customVar("terror"))));
        addToBot(new FgoNpAction(np));
    }
}
