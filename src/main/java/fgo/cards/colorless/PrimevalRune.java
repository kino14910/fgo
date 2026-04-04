package fgo.cards.colorless;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.powers.WeakPower;
import com.megacrit.cardcrawl.vfx.combat.BlizzardEffect;

import fgo.cards.FGOCard;

public class PrimevalRune extends FGOCard {
    public static final String ID = makeID(PrimevalRune.class.getSimpleName());

    public PrimevalRune() {
        super(ID, 1, CardType.SKILL, CardTarget.ALL_ENEMY, CardRarity.RARE, CardColor.COLORLESS);
        setMagic(2, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new VFXAction(new BlizzardEffect(5, AbstractDungeon.getMonsters().shouldFlipVfx()), Settings.FAST_MODE ? 0.25f : 1.0f));
        addToBot(new AllEnemyApplyPowerAction(p, magicNumber,
                monster -> new WeakPower(monster, magicNumber, false))
        );
        addToBot(new AllEnemyApplyPowerAction(p, magicNumber,
                monster -> new VulnerablePower(monster, magicNumber, false))
        );
    }
}


