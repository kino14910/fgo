package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInDiscardAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.HemokinesisEffect;

import fgo.cards.FGOCard;
import fgo.cards.status.CurseDisaster;

public class Canopus extends FGOCard {
    public static final String ID = makeID(Canopus.class.getSimpleName());

    public Canopus() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(15, 5);
        cardsToPreview = new CurseDisaster();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (m != null) {
            addToBot(new VFXAction(new HemokinesisEffect(p.hb.cX, p.hb.cY, m.hb.cX, m.hb.cY), 0.5f));
        }

        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        addToBot(new MakeTempCardInDiscardAction(cardsToPreview, 1));
    }
}


