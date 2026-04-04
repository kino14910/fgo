package fgo.ui.buttons;

import static fgo.FGOMod.uiPath;
import static fgo.utils.ModHelper.addToBot;

import java.util.ArrayList;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.megacrit.cardcrawl.actions.watcher.ChooseOneAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.Hitbox;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.MathHelper;
import com.megacrit.cardcrawl.helpers.TipHelper;
import com.megacrit.cardcrawl.helpers.input.InputHelper;
import com.megacrit.cardcrawl.localization.TutorialStrings;
import com.megacrit.cardcrawl.localization.UIStrings;
import com.megacrit.cardcrawl.ui.panels.AbstractPanel;
import com.megacrit.cardcrawl.vfx.ThoughtBubble;

import fgo.cards.optionCards.ReleaseNoblePhantasm;
import fgo.cards.optionCards.RepairSpiritOrigin;
import fgo.patches.RevivePatch;
import fgo.ui.panels.CommandSpellPanel;

public class CommandSpellButton extends AbstractPanel {
    private static final TutorialStrings tutorialStrings = CardCrawlGame.languagePack.getTutorialString("fgo:CommandSpell");
    private static final UIStrings uiStrings = CardCrawlGame.languagePack.getUIString("fgo:CommandSpell");

    private float scale = 1.0f;
    private final Hitbox hb;
    private final Color renderColor = Color.WHITE.cpy();

    public CommandSpellButton() {
        super(
            Settings.WIDTH - 128.0f * Settings.scale,
            Settings.HEIGHT - 320.0f * Settings.scale,
            Settings.WIDTH,
            Settings.HEIGHT - 320.0f * Settings.scale,
            8.0f * Settings.xScale,
            0.0f,
            null,
            true
        );
        hb = new Hitbox(
            Settings.WIDTH - 128.0f * Settings.scale,
            Settings.HEIGHT - 320.0f * Settings.scale,
            128.0f * Settings.scale,
            128.0f * Settings.scale
        );
    }

    @Override
    public void updatePositions() {
        super.updatePositions();
        hb.update();
        scale = MathHelper.scaleLerpSnap(scale, Settings.scale);
        
        if (hb.hovered) {
            AbstractDungeon.overlayMenu.hoveredTip = true;
            if (InputHelper.justClickedLeft) {
                InputHelper.justClickedLeft = false;
                chooseCommandSpell();
                CardCrawlGame.sound.playA("UI_CLICK_1", -0.1F);
            }
        }
    }

    /**
     * {@link RevivePatch}
     */
    public void chooseCommandSpell() {
        AbstractPlayer p = AbstractDungeon.player;

        if (CommandSpellPanel.commandSpellCount <= 0) {
            AbstractDungeon.effectList.add(new ThoughtBubble(p.dialogX, p.dialogY, 3.0f, uiStrings.TEXT[0], true));
            return;
        }

        ArrayList<AbstractCard> stanceChoices = new ArrayList<>();
        stanceChoices.add(new RepairSpiritOrigin());
        stanceChoices.add(new ReleaseNoblePhantasm());
        addToBot(new ChooseOneAction(stanceChoices));
        CommandSpellPanel.commandSpellCount--;
        CommandSpellPanel.CommandSpell = ImageMaster.loadImage(
            uiPath("CommandSpell/CommandSpell" + CommandSpellPanel.commandSpellCount + "")
        );
    }

    @Override
    public void render(SpriteBatch sb) {
        sb.setColor(renderColor);
        CommandSpellPanel.CommandSpell = ImageMaster.loadImage(
            uiPath("CommandSpell/CommandSpell" + CommandSpellPanel.commandSpellCount + "")
        );
        sb.draw(CommandSpellPanel.CommandSpell, current_x, current_y, 64.0f, 64.0f, 128.0f, 128.0f, scale, scale, 0.0f, 0, 0, 128, 128, false, false);

        hb.render(sb);

        if (hb.hovered) {
            scale = 1.2f * Settings.scale;
            CardCrawlGame.cursor.render(sb);
        }

        if (shouldRenderTip()) {
            TipHelper.renderGenericTip(
                1400.0f * Settings.xScale,
                Settings.HEIGHT - 256.0f * Settings.scale,
                tutorialStrings.LABEL[0],
                tutorialStrings.TEXT[0] + tutorialStrings.TEXT[1] + tutorialStrings.TEXT[2]
            );
        }
    }

    private boolean shouldRenderTip() {
        return !isHidden
                && hb != null
                && hb.hovered
                && !AbstractDungeon.isScreenUp
                && AbstractDungeon.getMonsters() != null
                && !AbstractDungeon.getMonsters().areMonstersDead();
    }
}
