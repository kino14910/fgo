package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.actions.utility.WaitAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.watcher.EndTurnDeathPower;
import com.megacrit.cardcrawl.vfx.combat.WeightyImpactEffect;

import fgo.cards.FGOCard;
import fgo.powers.CriticalDamageUpPower;

public class PeerlessStrike extends FGOCard {
    public static final String ID = makeID(PeerlessStrike.class.getSimpleName());

    public PeerlessStrike() {
        super(ID, 0, CardType.ATTACK, CardTarget.ENEMY, CardRarity.RARE);
        setDamage(30);
        tags.add(CardTags.STRIKE);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (m != null) {
            this.addToBot(new VFXAction(new WeightyImpactEffect(m.hb.cX, m.hb.cY)));
        }
        this.addToBot(new WaitAction(0.8f));
        this.addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.NONE));
        addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, 100)));
        if (!upgraded) {
            addToBot(new LoseHPAction(p, p, 99999));
        } else {
            addToBot(new ApplyPowerAction(p, p, new EndTurnDeathPower(p)));
        }
    }

    @Override
    public boolean canUse(AbstractPlayer p, AbstractMonster m) {
        boolean canUse = super.canUse(p, m);
        if (!canUse) {
            return false;
        } 

        if ((float) p.currentHealth > (float) p.maxHealth / 2.0f) {
            canUse = false;
            cantUseMessage = cardStrings.EXTENDED_DESCRIPTION[0];
        }

        return canUse;
    }

    @Override
    public void triggerOnGlowCheck() {
        AbstractPlayer p = AbstractDungeon.player;
        if ((float) p.currentHealth <= (float) p.maxHealth / 2.0f && p.currentHealth > 0) {
            glowColor = AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
        }
    }
}


