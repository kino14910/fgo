package fgo.characters;

import static fgo.FGOMod.characterPath;
import static fgo.FGOMod.makeID;
import static fgo.FGOMod.uiPath;
import static fgo.characters.CustomEnums.FGO_MASTER;

import java.util.ArrayList;
import java.util.Arrays;

import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.g2d.BitmapFont;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.EnergyManager;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.cutscenes.CutscenePanel;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.events.city.Vampires;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.helpers.Hitbox;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.ScreenShake;
import com.megacrit.cardcrawl.helpers.TipHelper;
import com.megacrit.cardcrawl.screens.CharSelectInfo;

import basemod.abstracts.CustomPlayer;
import fgo.cards.fgo.CharismaOfHope;
import fgo.cards.fgo.Defend;
import fgo.cards.fgo.DreamUponTheStars;
import fgo.cards.fgo.Strike;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.patches.MainMenuUIFgoPatch;
import fgo.patches.PictureSelectFgoPatch;
import fgo.relics.SuitcaseFgo;
import fgo.ui.panels.FGOConfig;
import fgo.utils.ModHelper;
import fgo.utils.Sounds;

public class Master extends CustomPlayer {
    private static final String[] ORB_TEXTURES = new String[] {
            uiPath("EPanel/layer5"),
            uiPath("EPanel/layer4"),
            uiPath("EPanel/layer3"),
            uiPath("EPanel/layer2"),
            uiPath("EPanel/layer1"),
            uiPath("EPanel/layer0"),
            uiPath("EPanel/layer5d"),
            uiPath("EPanel/layer4d"),
            uiPath("EPanel/layer3d"),
            uiPath("EPanel/layer2d"),
            uiPath("EPanel/layer1d")};
    private static final float[] LAYER_SPEED = new float[] { -40.0f, -32.0f, 20.0f, -20.0f, 0.0f, -10.0f, -8.0f, 5.0f, -5.0f, 0.0f };
    public static final Color SILVER = CardHelper.getColor(200, 200, 200);
    public static float FgoNp_BAR_HEIGHT = 20.0f * Settings.scale;
    private Hitbox FgoNPhb;
    public Color FgoNpBarColor1 = CardHelper.getColor(204, 128, 28);
    public Color FgoNpBarColor2 = CardHelper.getColor(238, 175, 10);
    public Color FgoNpBarColor3 = CardHelper.getColor(242, 236, 103);
    public Color FgoNpShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
    public Color FgoNpBgColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);
    public Color FgoNptextColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static final String[] TEXT = CardCrawlGame.languagePack.getCharacterString(makeID("Master")).TEXT;
    private static final String[] NPTEXT = CardCrawlGame.languagePack.getUIString(makeID("NPText")).TEXT;
    private float FgoNpWidth;
    private float FgoNpHideTimer = 1.0f;
    public static int fgoNp;
    
    public Master(String name) {
        super(name, FGO_MASTER, ORB_TEXTURES, uiPath("energyBlueVFX"), LAYER_SPEED, null, null);
        dialogX = drawX + 0.0f * Settings.scale;
        dialogY = drawY + 220.0f * Settings.scale;

        initializeClass(
                String.valueOf(MainMenuUIFgoPatch.refreshSkinFgo()),
                characterPath("shoulder2"), characterPath("shoulder1"),
                characterPath("fallen"),
                getLoadout(),
                0.0f, 5.0f,
                240.0f, 300.0f,
                new EnergyManager(3)
        );
    }

    @Override
    public ArrayList<String> getStartingDeck() {
        // int charIndex = MainMenuUIFgoPatch.modifierIndexes;
        ArrayList<String> retVal = new ArrayList<>(Arrays.asList(
            Strike.ID, Strike.ID, Strike.ID, Strike.ID,
            Defend.ID, Defend.ID, Defend.ID,
            CharismaOfHope.ID,
            DreamUponTheStars.ID
        ));
        return retVal;
    }

    @Override
    public ArrayList<String> getStartingRelics() {
        ArrayList<String> retVal = new ArrayList<>();
        retVal.add(SuitcaseFgo.ID);
        return retVal;
    }

    @Override
    public CharSelectInfo getLoadout() {
        //选人物界面的文字描述
        return new CharSelectInfo(
                TEXT[1],
                TEXT[2],
                76,
                76,
                0,
                99,
                5,
                this,
                getStartingRelics(),
                getStartingDeck(),
                false
        );
    }

    @Override
    public String getTitle(PlayerClass playerClass) { return TEXT[3]; }

    @Override
    public AbstractCard.CardColor getCardColor() {
        return FGOCardColor.FGO;
    }

    @Override
    public Color getCardRenderColor() {
        return SILVER;
    }

    @Override
    public AbstractCard getStartCardForEvent() {
        return new DreamUponTheStars();
    }

    @Override
    public Color getCardTrailColor() {
        return SILVER;
    }

    @Override
    public int getAscensionMaxHPLoss() {
        return 6;
    }

    @Override
    public BitmapFont getEnergyNumFont() {
        return FontHelper.energyNumFontBlue;
    }

    @Override
    public void doCharSelectScreenSelectEffect() {
        CardCrawlGame.mainMenuScreen.charSelectScreen.bgCharImg = PictureSelectFgoPatch.updateBgImg();
        CardCrawlGame.sound.playV(
            // LocalDate.now().getMonth() == Month.DECEMBER && 
            FGOConfig.enablePadoru ? Sounds.Padoru : Sounds.masterChoose, 0.8F);
        CardCrawlGame.screenShake.shake(ScreenShake.ShakeIntensity.MED, ScreenShake.ShakeDur.SHORT, true);
    }

    @Override
    public void updateOrb(int orbCount) {
        energyOrb.updateOrb(orbCount);
    }

    @Override
    public String getCustomModeCharacterButtonSoundKey() {
        return null;
    }

    @Override
    public String getLocalizedCharacterName() { return TEXT[4]; }

    @Override
    public AbstractPlayer newInstance() {
        return new Master(name);
    }

    @Override
    public Color getSlashAttackColor() {
        return SILVER;
    }

    @Override
    public AttackEffect[] getSpireHeartSlashEffect() {
        return new AttackEffect[] { AttackEffect.SLASH_HEAVY, AttackEffect.FIRE, AttackEffect.SLASH_DIAGONAL, AttackEffect.SLASH_HEAVY, AttackEffect.FIRE, AttackEffect.SLASH_DIAGONAL };
    }

    @Override
    public String getVampireText() { return Vampires.DESCRIPTIONS[1]; }

    @Override
    public String getSpireHeartText() {
        return TEXT[0];
    }

    @Override
    public void applyEndOfTurnTriggers() {
        super.applyEndOfTurnTriggers();
    }

    @Override
    public ArrayList<CutscenePanel> getCutscenePanels() {
        ArrayList<CutscenePanel> panels = new ArrayList<>(Arrays.asList(
            new CutscenePanel(characterPath("Victory1")),
            new CutscenePanel(characterPath("Victory2")),
            new CutscenePanel(characterPath("Victory3"))
        ));
        return panels;
    }

    @Override
    public void preBattlePrep() {
        super.preBattlePrep();
        Master.fgoNp = AbstractDungeon.player.hasRelic(SuitcaseFgo.ID) ? 20 : 0;
        TruthValueUpdatedEvent();
    }
    
    public void TruthValueUpdatedEvent() {
        int base = fgoNp > 200 ? 200 : (fgoNp > 100 ? 100 : 0);
        FgoNpWidth = hb.width * (fgoNp - base) / 100.0f;
    }

    @Override
    public void renderHealth(SpriteBatch sb) {
        super.renderHealth(sb);
        float x = hb.x;
        float y = hb.y + hb.height;
        
        // Check if mintySpire TotalIncomingDamage is enabled
        if (ModHelper.isMintySpireTIDEnabled()) {
            y += 40.0f * Settings.scale; // Adjust position upward
        }
        
        FgoNPhb = new Hitbox(x, y, hb_w, FgoNp_BAR_HEIGHT);
        FgoNPhb.render(sb);
        TruthValueBgRender(sb, x, y);
        renderTruthValueBar(sb, x);
        TruthValueText(sb);

        FgoNPhb.update();
        if (FgoNPhb.hovered) {
            if (!AbstractDungeon.isScreenUp) {
                TipHelper.renderGenericTip(hb.cX + hb.width / 2.0f + TIP_OFFSET_R_X, y + hb.height / 2.0f, NPTEXT[0], String.format(NPTEXT[1], FGOConfig.baseNPPerCost));
            }
            FgoNpHideTimer -= Gdx.graphics.getDeltaTime() * 4.0f;
            if (FgoNpHideTimer < 0.2f) {
                FgoNpHideTimer = 0.2f;
            }
        } else {
            FgoNpHideTimer += Gdx.graphics.getDeltaTime() * 4.0f;
            if (FgoNpHideTimer > 1.0f) {
                FgoNpHideTimer = 1.0f;
            }
        }
    }

    private void renderTruthValueBar(SpriteBatch sb, float x) {
        Color color = fgoNp > 200 ? FgoNpBarColor3 : (fgoNp > 100 ? FgoNpBarColor2 : FgoNpBarColor1);
        sb.setColor(color);

        sb.draw(ImageMaster.HEALTH_BAR_L, x - FgoNp_BAR_HEIGHT, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HEALTH_BAR_B, x, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, FgoNpWidth, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HEALTH_BAR_R, x + FgoNpWidth, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
    }

    private void TruthValueText(SpriteBatch sb) {
        float tmp = FgoNptextColor.a;
        FgoNptextColor.a *= FgoNpHideTimer;
        FontHelper.renderFontCentered(sb, FontHelper.healthInfoFont, Master.fgoNp + "%", hb.cX, FgoNPhb.cY, FgoNptextColor);
        FgoNptextColor.a = tmp;
    }

    private void TruthValueBgRender(SpriteBatch sb, float x, float y) {
        Color color = fgoNp > 200 ? FgoNpBarColor2 : (fgoNp > 100 ? FgoNpBarColor1 : FgoNpShadowColor);
        sb.setColor(color);

        //宝具值外框颜色。
        sb.draw(ImageMaster.HB_SHADOW_L, x - FgoNp_BAR_HEIGHT, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HB_SHADOW_B, x, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, hb.width, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HB_SHADOW_R, x + hb.width, FgoNPhb.cY - FgoNp_BAR_HEIGHT / 2.0f, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
        sb.setColor(FgoNpBgColor);
        //宝具值内框颜色。
        sb.draw(ImageMaster.HEALTH_BAR_L, x - FgoNp_BAR_HEIGHT, y, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HEALTH_BAR_B, x, y, hb.width, FgoNp_BAR_HEIGHT);
        sb.draw(ImageMaster.HEALTH_BAR_R, x + hb.width, y, 20.0f * Settings.scale, FgoNp_BAR_HEIGHT);
    }
}
