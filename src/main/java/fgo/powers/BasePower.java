package fgo.powers;

import java.util.Objects;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.graphics.g2d.TextureAtlas;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.helpers.FontHelper;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.utils.GeneralUtils;
import fgo.utils.TextureLoader;

public abstract class BasePower extends AbstractPower {
    private static PowerStrings getPowerStrings(String ID)
    {
        return CardCrawlGame.languagePack.getPowerStrings(ID);
    }
    protected AbstractCreature source;
    protected String[] DESCRIPTIONS;

    //Will not display if at 0. You can override renderAmount to render it however you want.
    //amount2 will not stack like the normal amount variable when stacking a power.
    public int amount2 = 0;
    protected Color redColor2 = Color.RED.cpy();
    protected Color greenColor2 = Color.GREEN.cpy();
    
   public boolean canGoNegative2 = false;

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner) {
        this(id, powerType, isTurnBased, owner, null, -1, null);
    }

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, String img) {
        this(id, powerType, isTurnBased, owner, null, -1, img);
    }

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, int amount) {
        this(id, powerType, isTurnBased, owner, null, amount, null);
    }

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, int amount, String img) {
        this(id, powerType, isTurnBased, owner, null, amount, img);
    }

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, AbstractCreature source, int amount) {
        this(id, powerType, isTurnBased, owner, source, amount, null);
    }

    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, AbstractCreature source, int amount, String img) {
        this(id, powerType, isTurnBased, owner, source, amount, img, true);
    }
    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, AbstractCreature source, int amount, String img, boolean initDescription) {
        this(id, powerType, isTurnBased, owner, source, amount, img, initDescription, true);
    }
    public BasePower(String id, PowerType powerType, boolean isTurnBased, AbstractCreature owner, AbstractCreature source, int amount, String img, boolean initDescription, boolean loadImage) {
        this.ID = id;
        this.isTurnBased = isTurnBased;

        PowerStrings strings = getPowerStrings(this.ID);
        this.name = strings.NAME;
        this.DESCRIPTIONS = strings.DESCRIPTIONS;

        this.owner = owner;
        this.source = source;
        this.amount = amount;
        this.type = powerType;

        if (loadImage) {
            String unPrefixed = Objects.nonNull(img) ? img : GeneralUtils.removePrefix(id);
            Texture normalTexture = TextureLoader.getPowerTexture(unPrefixed);
            Texture hiDefImage = TextureLoader.getHiDefPowerTexture(unPrefixed);

            if (hiDefImage != null) {
                region128 = new TextureAtlas.AtlasRegion(hiDefImage, 0, 0, hiDefImage.getWidth(), hiDefImage.getHeight());
                if (normalTexture != null)
                    region48 = new TextureAtlas.AtlasRegion(normalTexture, 0, 0, normalTexture.getWidth(), normalTexture.getHeight());
            } else {
                this.img = normalTexture;
                region48 = new TextureAtlas.AtlasRegion(normalTexture, 0, 0, normalTexture.getWidth(), normalTexture.getHeight());
            }
        }

        if (initDescription)
            this.updateDescription();
    }

    @Override
    public void renderAmount(SpriteBatch sb, float x, float y, Color c) {
        super.renderAmount(sb, x, y, c);

        if (this.amount2 != 0) {
            if (!this.isTurnBased) {
                float alpha = c.a;
                c = this.amount2 > 0 ? this.greenColor2 : this.redColor2;
                c.a = alpha;
                FontHelper.renderFontRightTopAligned(sb, FontHelper.powerAmountFont, Integer.toString(this.amount2), x, y + 15.0f * Settings.scale, this.fontScale, c);
            }
        }
    }

    @Override
    public void updateDescription() {
        description = String.format(DESCRIPTIONS[0], amount);
    }

    protected String formatDescriptionByQuantity(int amount) {
        return amount == 1 ? DESCRIPTIONS[0] : String.format(DESCRIPTIONS[1], amount);
    }
}