package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.AbsNoblePhantasmCard;
import fgo.powers.ArtsPerformancePower;
import fgo.powers.LoseArtsPerformancePower;

public class BeautifulJourney extends AbsNoblePhantasmCard {
    public static final String ID = makeID(BeautifulJourney.class.getSimpleName());

    public BeautifulJourney() {
        super(ID, CardType.ATTACK, CardTarget.ALL_ENEMY, 2);
        setDamage(24, 6);
        setNP(20);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new ArtsPerformancePower(p, 1)));
        addToBot(new ApplyPowerAction(p, p, new LoseArtsPerformancePower(p, 1)));

        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.SLASH_DIAGONAL));
        for (AbstractMonster mo : AbstractDungeon.getMonsters().monsters) {
            if (!mo.isDeadOrEscaped()) {
                addToBot(new FgoNpAction(np));
            }
        }
    }
}
