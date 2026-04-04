package fgo.patches;

import static fgo.FGOMod.NP_RATE_POWER_MULTIPLIER;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePrefixPatch;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.rooms.AbstractRoom;
import com.megacrit.cardcrawl.ui.panels.EnergyPanel;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.characters.Master;
import fgo.powers.NPRatePower;
import fgo.powers.SealNPPower;
import fgo.ui.panels.FGOConfig;

@SpirePatch(
    clz = AbstractCard.class,
    method = "renderCardTip"
)
public class PatchCostAmtKey {
    private static final String[] NPTEXT = CardCrawlGame.languagePack.getUIString("fgo:NPText").TEXT;

    @SpirePrefixPatch
    public static void Prefix(AbstractCard __instance, SpriteBatch sb) {
        AbstractPlayer p = AbstractDungeon.player;
        if (!(p instanceof Master) || __instance.isLocked || p.isDraggingCard || p.inSingleTargetMode) {
            return;
        }
        if (isInCombat() && p.hoveredCard == __instance && __instance.costForTurn > -2 && __instance.costForTurn != 0 && !p.hasPower(SealNPPower.POWER_ID) && __instance.color != FGOCardColor.NOBLE_PHANTASM) {
            int costModifier = getCostModifier();
            int costAmt = calculateCostAmount(__instance.costForTurn, costModifier);
            FontHelper.renderFontCentered(sb, FontHelper.topPanelInfoFont, ("+" + costAmt + "% " + NPTEXT[0]), __instance.hb.cX, (__instance.hb.height + 24.0f * Settings.scale), Color.WHITE.cpy());
        }
    }

    private static boolean isInCombat() {
        return AbstractDungeon.getCurrMapNode() != null && AbstractDungeon.getCurrRoom() != null && AbstractDungeon.getCurrRoom().phase == AbstractRoom.RoomPhase.COMBAT;
    }

    private static int getCostModifier() {
        boolean hasGoldLaw = AbstractDungeon.player.hasPower(NPRatePower.POWER_ID);
        return FGOConfig.baseNPPerCost * (hasGoldLaw ? NP_RATE_POWER_MULTIPLIER : 1);
    }

    private static int calculateCostAmount(int costForTurn, int costModifier) {
        return costForTurn == -1 ? EnergyPanel.totalCount * costModifier : costForTurn * costModifier;
    }
}

