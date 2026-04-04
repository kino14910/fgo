package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.WatersidePower;

public class SwanLake extends FGOCard {
    public static final String ID = makeID(SwanLake.class.getSimpleName());

    public SwanLake() { this(0); }
    
    public SwanLake(int upgrades) {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(10, 4);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.SLASH_DIAGONAL));
        addToBot(new ApplyPowerAction(p, p, new WatersidePower(p)));
    }
}


