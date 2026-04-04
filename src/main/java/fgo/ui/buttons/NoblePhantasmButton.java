package fgo.ui.buttons;

import static fgo.FGOMod.uiPath;
import static fgo.utils.ModHelper.addToBot;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
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
import com.megacrit.cardcrawl.ui.panels.AbstractPanel;

import fgo.action.NoblePhantasmSelectAction;
import fgo.characters.Master;
import fgo.powers.SealNPPower;
import fgo.utils.ModHelper;

public class NoblePhantasmButton extends AbstractPanel {
    private static final TutorialStrings tutorialStrings = CardCrawlGame.languagePack.getTutorialString("fgo:NoblePhantasm");
    private float scale = 1.0f;
    private final Hitbox hb;
    private final Color renderColor = Color.WHITE.cpy();
    private static final Texture NP_MAX = ImageMaster.loadImage(uiPath("np_max"));

    public NoblePhantasmButton() {
        super(
            0.0f,
            0.0f,
            -Settings.WIDTH,
            0.0f,
            8.0f * Settings.xScale,
            0.0f,
            null,
            true
        );
        hb = new Hitbox(
            0.0f,
            0.0f,
            64.0f * Settings.scale,
            64.0f * Settings.scale
        );
    }

    @Override
    public void updatePositions() {
        if (AbstractDungeon.player instanceof Master) {
            Master master = (Master) AbstractDungeon.player;
            float x = master.hb.x - 48.0f * Settings.scale;
            float y = master.hb.y + master.hb.height - 16.0f * Settings.scale + (ModHelper.isMintySpireTIDEnabled() ? 40.0f * Settings.scale : 0.0f);
            
            this.current_x = x;
            this.current_y = y;
            this.target_x = x;
            this.target_y = y;
            
            hb.move(
                master.hb.x - 64.0f * Settings.scale,
                master.hb.y + master.hb.height - 12.0f * Settings.scale + (ModHelper.isMintySpireTIDEnabled() ? 40.0f * Settings.scale : 0.0f)
            );
        }
        
        super.updatePositions();
        hb.update();
        scale = MathHelper.scaleLerpSnap(scale, Settings.scale);

        if (hb.hovered) {
            AbstractDungeon.overlayMenu.hoveredTip = true;
            if (InputHelper.justClickedLeft) {
                InputHelper.justClickedLeft = false;
                chooseNobleCard();
                CardCrawlGame.sound.playA("UI_CLICK_1", -0.1F);
            }
        }
    }

    private void chooseNobleCard() {
        AbstractPlayer p = AbstractDungeon.player;

        if (Master.fgoNp >= 100) {
            Master.fgoNp = 0;
            ((Master) p).TruthValueUpdatedEvent();
            addToBot(new NoblePhantasmSelectAction());
        }
    }

    @Override
    public void render(SpriteBatch sb) {
        sb.setColor(renderColor);

        if (!AbstractDungeon.player.hasPower(SealNPPower.POWER_ID) && Master.fgoNp >= 100 && !AbstractDungeon.isScreenUp) {
            sb.draw(NP_MAX, current_x, current_y, 32.0f, 32.0f, 64.0f, 64.0f, scale, scale, 0.0f, 0, 0, 64, 64, false, false);
        }

        hb.render(sb);

        if (shouldRenderTip() && AbstractDungeon.player instanceof Master) {
            Master master = (Master) AbstractDungeon.player;
            float tipYOffset = 0.0f;
            if (ModHelper.isMintySpireTIDEnabled()) {
                tipYOffset = 40.0f * Settings.scale; // Adjust tip position upward for MintySpireTID
            }
            
            TipHelper.renderGenericTip(
                master.hb.x - 128.0f * Settings.scale,
                master.hb.y + master.hb.height - 32.0f * Settings.scale + tipYOffset,
                tutorialStrings.LABEL[0],
                tutorialStrings.TEXT[0] + tutorialStrings.TEXT[1]
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
