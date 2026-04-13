package fgo.cards.noblecards;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.AbsNoblePhantasmCard;

public class ChaosLabyrinthos extends AbsNoblePhantasmCard {
    public static final String ID = makeID(ChaosLabyrinthos.class.getSimpleName());

    public ChaosLabyrinthos() {
        super(ID, CardType.SKILL, CardTarget.ALL_ENEMY, 1);
        setMagic(12);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new AllEnemyApplyPowerAction(p, -magicNumber,
                monster -> new StrengthPower(monster, -magicNumber)));
    }
}
