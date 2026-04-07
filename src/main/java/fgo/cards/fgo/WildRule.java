package fgo.cards.fgo;

import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.vfx.combat.BiteEffect;

import fgo.cards.FGOCard;

public class WildRule extends FGOCard {
    public static final String ID = makeID(WildRule.class.getSimpleName());
    boolean hasVulnerable = false;
    
    public WildRule() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(10, 4);
        setMagic(1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(new BiteEffect(m.hb.cX, m.hb.cY - 40.0f * Settings.scale, Settings.GOLD_COLOR.cpy()), Settings.FAST_MODE ? 0.1f : 0.3f));

        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.NONE));
        addToBot(new HealAction(p, p, 3));
        if (getPowerCount(m, StrengthPower.POWER_ID) > 0) {
            addToBot(new ApplyPowerAction(m, p, new StrengthPower(m, -1)));
            addToBot(new ApplyPowerAction(m, p, new VulnerablePower(m, magicNumber, false)));
        }
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
        for (AbstractMonster m : AbstractDungeon.getMonsters().monsters) {
            if (getPowerCount(m, StrengthPower.POWER_ID) > 0) {
                glowColor = AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
                hasVulnerable = true;
                break;
            }
        }
    }
}


