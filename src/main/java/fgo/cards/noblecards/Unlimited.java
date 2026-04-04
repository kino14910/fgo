package fgo.cards.noblecards;

import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.FadeWipeParticle;

import fgo.FGOMod;
import fgo.cards.AbsNoblePhantasmCard;
import fgo.effects.ChangeSceneEffect;
import fgo.powers.UnlimitedPower;

public class Unlimited extends AbsNoblePhantasmCard {
    public static final String ID = makeID(Unlimited.class.getSimpleName());

    public Unlimited() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
        setMagic(1, 1);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        AbstractDungeon.topLevelEffects.add(new FadeWipeParticle());
        addToBot(new VFXAction(new ChangeSceneEffect(ImageMaster.loadImage(FGOMod.vfxPath("UnlimitedBg")))));
        CardCrawlGame.music.silenceTempBgmInstantly();
        CardCrawlGame.music.silenceBGMInstantly();
        AbstractDungeon.getCurrRoom().playBgmInstantly("UBW_Extended.mp3");
        addToBot(new ApplyPowerAction(p, p, new UnlimitedPower(p, magicNumber)));
    }
}
