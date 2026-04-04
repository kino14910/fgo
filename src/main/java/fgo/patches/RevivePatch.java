package fgo.patches;

import static fgo.characters.CustomEnums.FGO_MASTER;
import static fgo.utils.ModHelper.addToBot;

import com.evacipated.cardcrawl.modthespire.lib.LineFinder;
import com.evacipated.cardcrawl.modthespire.lib.Matcher;
import com.evacipated.cardcrawl.modthespire.lib.SpireInsertLocator;
import com.evacipated.cardcrawl.modthespire.lib.SpireInsertPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpireReturn;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.rooms.AbstractRoom;

import fgo.action.FgoNpAction;
import fgo.powers.DeathOfDeathPower;
import fgo.powers.GutsPower;
import fgo.powers.IndomitablePower;
import fgo.powers.NonStackableGutsPower;
import fgo.powers.SpringOfFirePower;
import fgo.relics.SaveStone;
import fgo.ui.panels.CommandSpellPanel;
import javassist.CtBehavior;

public class RevivePatch {
    @SpirePatch(clz = AbstractPlayer.class, method = "damage")
    public static class GutsPowerPatch {
        @SpireInsertPatch(locator = GutsPowerLocator.class)
        public static SpireReturn<Void> Insert(AbstractPlayer p) {
            String[] powerIds = {
                SpringOfFirePower.POWER_ID,
                DeathOfDeathPower.POWER_ID,
                IndomitablePower.POWER_ID
            };

            if (p.hasPower(GutsPower.POWER_ID) || p.hasPower(NonStackableGutsPower.POWER_ID)) {
                p.getPower(p.hasPower(NonStackableGutsPower.POWER_ID) 
                    ? NonStackableGutsPower.POWER_ID 
                    : GutsPower.POWER_ID
                ).onSpecificTrigger();

                for (String id : powerIds) {
                    if (p.hasPower(id)) {
                        p.getPower(id).onSpecificTrigger();
                    }
                }
                return SpireReturn.Return(null);
            }
            if (p.hasRelic(SaveStone.ID)) {
                p.currentHealth = 0;
                p.getRelic(SaveStone.ID).onTrigger();
            }
            return SpireReturn.Continue();
        }

        private static class GutsPowerLocator extends SpireInsertLocator {
            @Override
            public int[] Locate(CtBehavior ctBehavior) throws Exception {
                // 匹配 "hasRelic" 方法调用，即 "if (!this.hasRelic("Mark of the Bloom"))"
                Matcher.MethodCallMatcher methodCallMatcher = new Matcher.MethodCallMatcher(AbstractPlayer.class, "hasRelic");
                return LineFinder.findInOrder(ctBehavior, methodCallMatcher);
            }
        }
    }

    
    @SpirePatch(clz = AbstractPlayer.class, method = "damage")
    public static class CommandSpellPatch {
        @SpireInsertPatch(locator = CommandSpellLocator.class)
        public static SpireReturn<Void> Insert(AbstractPlayer p) {
            if (CommandSpellPanel.commandSpellCount == 3
                    && AbstractDungeon.currMapNode != null
                    && AbstractDungeon.getCurrRoom().phase == AbstractRoom.RoomPhase.COMBAT
                    && AbstractDungeon.player.chosenClass == FGO_MASTER
                    ) {
                CommandSpellPanel.commandSpellCount = 0;
                p.heal(p.maxHealth, true);
                addToBot(new FgoNpAction(300));
                return SpireReturn.Return(null);
            }
            return SpireReturn.Continue();
        }

        private static class CommandSpellLocator extends SpireInsertLocator {
            @Override
            public int[] Locate(CtBehavior ctBehavior) throws Exception {
                // 匹配 "isDead" 字段赋值，即 "this.isDead = true;"
                Matcher.FieldAccessMatcher fieldAccessMatcher = new Matcher.FieldAccessMatcher(AbstractPlayer.class, "isDead");
                return LineFinder.findInOrder(ctBehavior, fieldAccessMatcher);
            }
        }
    }
}
