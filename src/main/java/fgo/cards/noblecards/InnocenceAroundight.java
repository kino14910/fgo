package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInDrawPileAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.cards.tempCards.RayHorizon;
import fgo.powers.NPRatePower;

public class InnocenceAroundight extends AbsNoblePhantasmCard {
    public static final String ID = makeID(InnocenceAroundight.class.getSimpleName());

    public InnocenceAroundight() {
        super(ID, CardType.ATTACK, CardTarget.ENEMY, 1);
        setDamage(32, 8);
        setMagic(3);
        cardsToPreview = new RayHorizon();
        cardsToPreview.upgrade();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        addToBot(new ApplyPowerAction(p, p, new NPRatePower(p, magicNumber)));
        addToBot(new MakeTempCardInDrawPileAction(cardsToPreview, 1, true, true, false));
    }
}
