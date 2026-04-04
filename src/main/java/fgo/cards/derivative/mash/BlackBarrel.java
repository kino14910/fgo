package fgo.cards.derivative.mash;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;

public class BlackBarrel extends FGOCard {
    public static final String ID = makeID(BlackBarrel.class.getSimpleName());
    private final String upgradeName;

    public BlackBarrel() {
        super(ID, 0, CardType.ATTACK, CardTarget.ENEMY, CardRarity.SPECIAL, FGOCardColor.FGO_DERIVATIVE);
        upgradeName = cardStrings.EXTENDED_DESCRIPTION[0];
        setDamage(9, 3);
        setExhaust();
    }

    @Override
    protected void upgradeName() {
        timesUpgraded++;
        upgraded = true;
        name = upgradeName;
        initializeTitle();
    }

    @Override
    public void calculateCardDamage(AbstractMonster mo) {
        int realBaseDamage = this.baseDamage;
        if (!mo.isDying && !mo.isDead && mo.currentBlock > 0) {
            this.baseDamage += mo.currentBlock;
        }
        super.calculateCardDamage(mo);
        this.baseDamage = realBaseDamage;
        this.isDamageModified = this.damage != this.baseDamage;
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, this.damage), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
    }
}
