package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.FlameBarrierEffect;

import fgo.cards.FGOCard;
import fgo.powers.CriticalDamageUpPower;
import fgo.powers.StarPower;

public class KnightoftheLake extends FGOCard {
    public static final String ID = makeID(KnightoftheLake.class.getSimpleName());

    public KnightoftheLake() {
        super(ID, 2, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON);
        setMagic(50, 50);
        setStar(10);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(p, new FlameBarrierEffect(p.hb.cX, p.hb.cY), Settings.FAST_MODE ? 0.1F : 0.5f));

        if (!p.hasPower(CriticalDamageUpPower.POWER_ID)) {
            addToBot(new ApplyPowerAction(p, p, new CriticalDamageUpPower(p, magicNumber)));
        } else {
            addToBot(new ApplyPowerAction(p, p, new StarPower(p, star)));
        }
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = AbstractDungeon.player.hasPower(CriticalDamageUpPower.POWER_ID)
                ? AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy()
                : AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
    }
}


