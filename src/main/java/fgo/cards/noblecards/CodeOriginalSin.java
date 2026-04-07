package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.actions.watcher.JudgementAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.AbsNoblePhantasmCard;

public class CodeOriginalSin extends AbsNoblePhantasmCard {
    public static final String ID = makeID(CodeOriginalSin.class.getSimpleName());

    public CodeOriginalSin() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 1);
        setDamage(32, 8);
        setMagic(10);
        setNP(20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new LoseHPAction(m, p, damage));
        addToBot(new FgoNpAction(np));
        addToBot(new JudgementAction(m, 15));
    }
}
