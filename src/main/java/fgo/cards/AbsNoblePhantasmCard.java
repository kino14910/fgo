package fgo.cards;

import static fgo.FGOMod.cardPath;

import com.badlogic.gdx.graphics.Color;
import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.CommonKeywordIconsField;
import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.PurgeField;
import com.evacipated.cardcrawl.mod.stslib.patches.FlavorText;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.characters.CustomEnums.FGOCardColor;
import fgo.hexui_lib.util.RenderImageLayer;
import fgo.hexui_lib.util.TextureLoader;
import fgo.powers.NPOverChargePower;
import fgo.ui.panels.FGOConfig;

public abstract class AbsNoblePhantasmCard extends FateMagineerCard {
    public AbsNoblePhantasmCard(String id, AbstractCard.CardType type, AbstractCard.CardTarget target) {
        this(id, type, target, -2);
    }
    
    public AbsNoblePhantasmCard(String id, AbstractCard.CardType type, AbstractCard.CardTarget target, int cost) {
        super(id, FGOConfig.enableNoCostNoblePhantasm ? -2 : cost, type, target, AbstractCard.CardRarity.SPECIAL, FGOCardColor.NOBLE_PHANTASM);
        setSelfRetain();
        PurgeField.purge.set(this, true);
        CommonKeywordIconsField.useIcons.set(this, false);
        FlavorText.AbstractCardFlavorFields.textColor.set(this, Color.CHARTREUSE);
        FlavorText.AbstractCardFlavorFields.flavorBoxType.set(this, FlavorText.boxType.TRADITIONAL);
        
        cardArtLayers512.add(new RenderImageLayer(TextureLoader.getTexture(noblePath(this.getClass().getSimpleName()))));
        cardArtLayers1024.add(new RenderImageLayer(TextureLoader.getTexture(noblePath(this.getClass().getSimpleName() + "_p"))));
    }

    public static String noblePath(String file) {
        return cardPath("noble/" + file);
    }

    @Override
    public boolean canUpgrade() {
        return true;
    }
    
    @Override
    public void upgrade() {
        timesUpgraded++;
        upgraded = true;
        name = cardStrings.NAME + "+" + (timesUpgraded == 0 ? "" : timesUpgraded);
        initializeTitle();

        if (upgradeDamage)
            upgradeDamage(damageUpgrade);

        if (upgradeBlock)
            upgradeBlock(blockUpgrade);

        if (upgradeMagic)
            upgradeMagicNumber(magicUpgrade);

        if (upgradeNP)
            upgradeMagicNumber(npUpgrade);

        if (upgradeStar)
            upgradeMagicNumber(starUpgrade);

        for (LocalVarInfo var : cardVariables.values()) {
            upgradeCustomVar(var);
        }

        if (baseExhaust ^ upgExhaust)
            exhaust = upgExhaust;

        initializeDescription();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(1)));
    }
}