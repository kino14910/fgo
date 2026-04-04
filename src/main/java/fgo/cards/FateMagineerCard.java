package fgo.cards;

import static fgo.FGOMod.imagePath;
import static fgo.utils.GeneralUtils.removePrefix;
import static fgo.utils.TextureLoader.getCardTextureString;

import java.util.ArrayList;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.evacipated.cardcrawl.modthespire.lib.SpireOverride;

import fgo.hexui_lib.interfaces.CustomCardPortrait;
import fgo.hexui_lib.interfaces.CustomCardTypeLocation;
import fgo.hexui_lib.util.BetaPortraitGenerator;
import fgo.hexui_lib.util.FloatPair;
import fgo.hexui_lib.util.RenderImageLayer;
import fgo.hexui_lib.util.RenderLayer;
import fgo.hexui_lib.util.TextureLoader;

public abstract class FateMagineerCard extends FGOCard implements CustomCardPortrait, CustomCardTypeLocation {
    public static ArrayList<RenderLayer> outerCircuits512 = new ArrayList<>();
    public static ArrayList<RenderLayer> outerCircuits1024 = new ArrayList<>();
    public static ArrayList<RenderLayer> outerMagic512 = new ArrayList<>();
    public static ArrayList<RenderLayer> outerMagic1024 = new ArrayList<>();
    public static boolean decoRenderLayersInitialized = false;
    public static RenderLayer improvementSlotsPanel512;
    public static RenderLayer improvementSlotsPanel1024;
    private final ArrayList<RenderLayer> portraitLayers512 = new ArrayList<>();
    private final ArrayList<RenderLayer> portraitLayers1024 = new ArrayList<>();
    public ArrayList<RenderLayer> cardArtLayers512 = new ArrayList<>();
    public ArrayList<RenderLayer> cardArtLayers1024 = new ArrayList<>();
    
    public FateMagineerCard(String ID, int cost, CardType cardType, CardTarget target, CardRarity rarity, CardColor color) {
        super(ID, cost, cardType, target, rarity, color, getCardTextureString(removePrefix(ID), cardType));
        initializeDecoRenderLayers();
    }
//    public FateMagineerCard(final String id, final String name, final String img, final int cost, final String rawDescription,
//                            final CardType type, final CardColor color,
//                            final CardRarity rarity, final CardTarget target) {
//        super(id, name, img, cost, rawDescription, type, color, rarity, target);
//
//        initializeDecoRenderLayers();
//    }

    public static String nobleResourcePath(String file) {
        return imagePath("NobleResources/" + file);
    }

    private void initializeDecoRenderLayers() {
        decoRenderLayersInitialized = true;
    }

    @Override
    public ArrayList<RenderLayer> getPortraitLayers512() {
        portraitLayers512.clear();
        addCardArtLayers512(portraitLayers512);
        
        portraitLayers512.add(new RenderImageLayer(TextureLoader.getTexture(this.description.size() >= 4 ? nobleResourcePath("512/desc_shadow") : nobleResourcePath("512/desc_shadow_small"))));
        portraitLayers512.add(new RenderImageLayer(TextureLoader.getTexture(nobleResourcePath("512/" + type.toString().toLowerCase() + "_common"))));
        return portraitLayers512;
    }

    @Override
    public ArrayList<RenderLayer> getPortraitLayers1024() {
        portraitLayers1024.clear();
        addCardArtLayers1024(portraitLayers1024);
        portraitLayers1024.add(new RenderImageLayer(TextureLoader.getTexture(nobleResourcePath("1024/desc_shadow"))));
        portraitLayers1024.add(new RenderImageLayer(TextureLoader.getTexture(nobleResourcePath("1024/" + type.toString().toLowerCase() + "_common"))));
        return portraitLayers1024;
    }

    @Override
    public FloatPair getCardTypeLocation(FloatPair pair, boolean isBigCard) {
        pair.y += isBigCard ? 432f : 224f;
        return pair;
    }

    protected void generateBetaArt() {
        cardArtLayers512 = BetaPortraitGenerator.generate(cardID, false);
        cardArtLayers1024 = BetaPortraitGenerator.generate(cardID, true);
    }

    public void addCardArtLayers512(ArrayList<RenderLayer> portraitLayers) {
        portraitLayers.addAll(cardArtLayers512);
    }

    public void addCardArtLayers1024(ArrayList<RenderLayer> portraitLayers) {
        portraitLayers.addAll(cardArtLayers1024);
    }

    @SpireOverride
	protected void renderBannerImage(SpriteBatch sb, float x, float y) {
    }
}