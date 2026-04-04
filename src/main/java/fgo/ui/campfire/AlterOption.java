package fgo.ui.campfire;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.MathHelper;
import com.megacrit.cardcrawl.ui.campfire.AbstractCampfireOption;

import basemod.ReflectionHacks;
import fgo.FGOMod;
import fgo.vfx.AlterOptionEffect;

public class AlterOption extends AbstractCampfireOption {
    public static final String[] TEXT = (CardCrawlGame.languagePack.getUIString(makeID(AlterOption.class.getSimpleName()))).TEXT;
    public static boolean usedIdentify = false;

    public AlterOption() {
        label = TEXT[0];
        description = TEXT[1];
        img = ImageMaster.loadImage(FGOMod.uiPath("tune"));
        usedIdentify = false;
    }

    @Override
    public void useOption() {
        if (usable) AbstractDungeon.effectList.add(new AlterOptionEffect());
    }

    @Override
    public void update() {
        if (usable && usedIdentify) {
            usable = false;
        }

        float hackScale = ((Float) ReflectionHacks.getPrivate(this, AbstractCampfireOption.class, "scale")).floatValue();

        if (hb.hovered) {
            if (!hb.clickStarted) {
                ReflectionHacks.setPrivate(this, AbstractCampfireOption.class, "scale", MathHelper.scaleLerpSnap(hackScale, Settings.scale));
                ReflectionHacks.setPrivate(this, AbstractCampfireOption.class, "scale", MathHelper.scaleLerpSnap(hackScale, Settings.scale));
            } else {
                ReflectionHacks.setPrivate(this, AbstractCampfireOption.class, "scale", MathHelper.scaleLerpSnap(hackScale, 0.9f * Settings.scale));
            }
        } else {
            ReflectionHacks.setPrivate(this, AbstractCampfireOption.class, "scale", MathHelper.scaleLerpSnap(hackScale, 0.9f * Settings.scale));
        }
        super.update();
    }
}