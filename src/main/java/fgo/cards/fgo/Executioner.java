package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;

public class Executioner extends FGOCard {
    public static final String ID = makeID(Executioner.class.getSimpleName());

    public Executioner() {
        super(ID, 0, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(7, 4);
        setMagic(1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.NONE));
        addToBot(new ApplyPowerAction(m, p, new StrengthPower(m, magicNumber)));
        addToBot(new GainEnergyAction(1));
    }
}


