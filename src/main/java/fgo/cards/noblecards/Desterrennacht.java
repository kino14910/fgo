package fgo.cards.noblecards;

import static fgo.characters.CustomEnums.FGO_Foreigner;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.ForeignerPower;
import fgo.powers.StarRegenPower;
import fgo.powers.TerrorPower;

public class Desterrennacht extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Desterrennacht.class.getSimpleName());

    public Desterrennacht() {
        super(ID, CardType.POWER, CardTarget.ALL_ENEMY, 2);
        setMagic(2, 1);
        setCustomVar("CriticalDamage", 60, 10);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new AllEnemyApplyPowerAction(p, magicNumber,
            monster -> new TerrorPower(monster, 3, 60))
        );
        addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, magicNumber)));
        addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, customVar("CriticalDamage"))));
        addToBot(new ApplyPowerAction(p, p, new ForeignerPower(p, 100)));
        addToBot(new ApplyPowerAction(p, p, new StarRegenPower(p, 10)));
    }
}
