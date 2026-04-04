package fgo.patches;

import java.lang.reflect.Field;
import java.util.Objects;

import com.badlogic.gdx.Gdx;
import com.evacipated.cardcrawl.modthespire.lib.SpireInsertPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpireReturn;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.utility.TextAboveCreatureAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.powers.AbstractPower;

import basemod.ReflectionHacks;
import fgo.powers.CursePower;
import fgo.relics.MidsummerNightDream;

@SpirePatch(
    clz = ApplyPowerAction.class,
    method = "update"
)
public class MidsummerNightPatch {
    private static final String[] TEXT = CardCrawlGame.languagePack.getUIString("fgo:MidsummerNightDream").TEXT;
    
    @SpireInsertPatch
            (rloc = 35)
    public static SpireReturn<Void> Insert(ApplyPowerAction __instance) {
        AbstractPower powerToApply = ReflectionHacks.getPrivate(__instance, ApplyPowerAction.class, "powerToApply");
        if (AbstractDungeon.player.hasRelic(MidsummerNightDream.ID) && __instance.target.isPlayer && powerToApply.ID.equals(CursePower.POWER_ID)) {
            AbstractDungeon.player.getRelic(MidsummerNightDream.ID).flash();
            AbstractDungeon.actionManager.addToTop(new TextAboveCreatureAction(__instance.target, TEXT[0]));
            float duration = (Float) Objects.requireNonNull(getPrivateInherited(__instance, ApplyPowerAction.class, "duration"));
            duration -= Gdx.graphics.getDeltaTime();
            ReflectionHacks.setPrivateInherited(__instance, ApplyPowerAction.class, "duration", duration);
            return SpireReturn.Return(null);
        }
        return SpireReturn.Continue();
    }

    public static Object getPrivateInherited(Object obj, Class<?> objClass, String fieldName) {
        try {
            Field targetField = objClass.getSuperclass().getDeclaredField(fieldName);
            targetField.setAccessible(true);
            return targetField.get(obj);
        } catch (Exception exception) {
            return null;
        }
    }
}
