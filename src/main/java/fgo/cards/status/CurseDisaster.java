package fgo.cards.status;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.combat.ShockWaveEffect;

import fgo.cards.FGOCard;
import fgo.powers.CursePower;

public class CurseDisaster extends FGOCard {
    public static final String ID = makeID(CurseDisaster.class.getSimpleName());

    public CurseDisaster() {
        super(ID, -2, CardType.STATUS, CardTarget.NONE, CardRarity.COMMON, CardColor.COLORLESS);
        setMagic(1);
        setEthereal();
    }

    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) { onChoseThisOption(); }

    @Override
    public void triggerWhenDrawn() {
        AbstractPlayer p = AbstractDungeon.player;
        addToBot(new SFXAction("ATTACK_PIERCING_WAIL"));
        addToBot(new VFXAction(p, new ShockWaveEffect(p.hb.cX, p.hb.cY, Settings.GREEN_TEXT_COLOR, ShockWaveEffect.ShockWaveType.CHAOTIC), Settings.FAST_MODE ? 0.3f : 1.5f));
        addToBot(new ApplyPowerAction(p, p, new CursePower(p, p, magicNumber)));
        addToBot(new AllEnemyApplyPowerAction(p, magicNumber,
                monster -> new CursePower(monster, p, magicNumber))
        );
    }
}
