package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.CleaveEffect;

import fgo.cards.FGOCard;
import fgo.powers.StarPower;

public class WarriorsBlade extends FGOCard {
    public static final String ID = makeID(WarriorsBlade.class.getSimpleName());
    
    public WarriorsBlade() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(2);
        setMagic(4, 1);
        setStar(6);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction("ATTACK_HEAVY"));
        addToBot(new VFXAction(p, new CleaveEffect(), 0.1F));
        for (int i = 0; i < magicNumber; i++) {
            addToBot(new DamageAction(m, new DamageInfo(p, damage, DamageInfo.DamageType.NORMAL), AbstractGameAction.AttackEffect.SLASH_VERTICAL));
        }
        addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
    }
}


