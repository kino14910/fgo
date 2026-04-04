package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.LightningEffect;

import fgo.cards.FGOCard;
import fgo.powers.NPRatePower;

public class TeslaCoil extends FGOCard {
    public static final String ID = makeID(TeslaCoil.class.getSimpleName());

    public TeslaCoil() {
        super(ID, 0, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(5, 2);
        setMagic(3, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction("THUNDERCLAP", 0.05F));
        if (!m.isDeadOrEscaped()) {
            addToBot(new VFXAction(new LightningEffect(m.drawX, m.drawY), 0.05F));
        }

        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.NONE, true));
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, magicNumber)));
    }
}


