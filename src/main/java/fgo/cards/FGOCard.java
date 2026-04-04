package fgo.cards;

import static fgo.characters.CustomEnums.FGOCardColor.FGO;
import static fgo.utils.GeneralUtils.removePrefix;
import static fgo.utils.TextureLoader.getCardTextureString;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Map;

import com.badlogic.gdx.graphics.Color;
import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.CommonKeywordIconsField;
import com.evacipated.cardcrawl.mod.stslib.patches.FlavorText;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.CardStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import basemod.BaseMod;
import basemod.abstracts.CustomCard;
import basemod.abstracts.DynamicVariable;
import fgo.FGOMod;
import fgo.utils.CardStats;
import fgo.utils.TriFunction;

public abstract class FGOCard extends CustomCard {
    final private static Map<String, DynamicVariable> customVars = new HashMap<>();

    protected static String makeID(String name) {
        return FGOMod.makeID(name);
    }

    protected final CardStrings cardStrings;

    protected boolean upgradesDescription;

    protected int baseCost;

    protected boolean upgradeCost;
    protected int costUpgrade;

    protected boolean upgradeDamage;
    protected boolean upgradeBlock;
    protected boolean upgradeMagic;

    protected int damageUpgrade;
    protected int blockUpgrade;
    protected int magicUpgrade;

    protected boolean baseExhaust = false;
    protected boolean upgExhaust = false;
    protected boolean baseEthereal = false;
    protected boolean upgEthereal = false;
    protected boolean baseInnate = false;
    protected boolean upgInnate = false;
    protected boolean baseRetain = false;
    protected boolean upgRetain = false;

    public boolean upgradeNP;
    public boolean upgradeStar;

    public int npUpgrade;
    public int starUpgrade;

    public int baseNP;
    public int baseStar;

    public int np;
    public int star;

    public boolean isModified;

    protected boolean upgradedNP;
    protected boolean upgradedStar;

    final protected Map<String, LocalVarInfo> cardVariables = new HashMap<>();

    public FGOCard(String ID, CardStats INFO) {
        this(ID, INFO, getCardTextureString(removePrefix(ID), INFO.cardType));
    }

    public FGOCard(String ID, CardStats INFO, String cardImage) {
        this(ID, INFO.baseCost, INFO.cardType, INFO.cardTarget, INFO.cardRarity, INFO.cardColor, cardImage);
    }

    public FGOCard(String ID, int cost, CardType cardType, CardTarget target, CardRarity rarity) {
        this(ID, cost, cardType, target, rarity, FGO);
    }

    public FGOCard(String ID, int cost, CardType cardType, CardTarget target, CardRarity rarity, CardColor color) {
        this(ID, cost, cardType, target, rarity, color, getCardTextureString(removePrefix(ID), cardType));
    }

    public FGOCard(String ID, int cost, CardType cardType, CardTarget target, CardRarity rarity, CardColor color,
            String cardImage) {
        super(ID, getName(ID), cardImage, cost, getInitialDescription(ID), cardType, color, rarity, target);
        cardStrings = CardCrawlGame.languagePack.getCardStrings(cardID);
        originalName = cardStrings.NAME;

        baseCost = cost;

        upgradesDescription = cardStrings.UPGRADE_DESCRIPTION != null;
        upgradeCost = false;
        upgradeDamage = false;
        upgradeBlock = false;
        upgradeMagic = false;

        costUpgrade = cost;
        damageUpgrade = 0;
        blockUpgrade = 0;
        magicUpgrade = 0;
        
        np = baseNP = npUpgrade = 0;
        upgradeNP = false;
        star = baseStar = starUpgrade = 0; 
        upgradeStar = false;

        if (target == CardTarget.ALL_ENEMY && type == CardType.ATTACK) {
            isMultiDamage = true;
        }
        
        FlavorText.AbstractCardFlavorFields.textColor.set(this, Color.CHARTREUSE);
        FlavorText.AbstractCardFlavorFields.flavorBoxType.set(this, FlavorText.boxType.TRADITIONAL);

        setCustomVar("NP", baseNP, npUpgrade);
        setCustomVar("S", baseStar, starUpgrade);
        CommonKeywordIconsField.useIcons.set(this, true);
    }

    private static String getName(String ID) {
        return CardCrawlGame.languagePack.getCardStrings(ID).NAME;
    }

    private static String getInitialDescription(String ID) {
        return CardCrawlGame.languagePack.getCardStrings(ID).DESCRIPTION;
    }

    // Methods meant for constructor use
    protected final void setDamage(int damage) {
        setDamage(damage, 0);
    }

    protected final void setDamage(int damage, int damageUpgrade) {
        this.baseDamage = this.damage = damage;
        if (damageUpgrade != 0) {
            this.upgradeDamage = true;
            this.damageUpgrade = damageUpgrade;
        }
    }

    protected final void setBlock(int block) {
        setBlock(block, 0);
    }

    protected final void setBlock(int block, int blockUpgrade) {
        this.baseBlock = this.block = block;
        if (blockUpgrade != 0) {
            this.upgradeBlock = true;
            this.blockUpgrade = blockUpgrade;
        }
    }

    protected final void setMagic(int magic) {
        setMagic(magic, 0);
    }

    protected final void setMagic(int magic, int magicUpgrade) {
        this.baseMagicNumber = this.magicNumber = magic;
        if (magicUpgrade != 0) {
            this.upgradeMagic = true;
            this.magicUpgrade = magicUpgrade;
        }
    }
    
    public int getNP() {
        return this.np;
    }

    public int getStar() {
        return this.star;
    }

    protected final void setNP(int np) { setNP(np, 0); }

    protected final void setNP(int np, int npUpgrade) {
        this.baseNP = this.np = np;
        if (npUpgrade != 0) {
            this.upgradeNP = true;
            this.npUpgrade = npUpgrade;
        }
    }

    protected final void upgradeNP(int amount) {
        this.np = this.baseNP += amount;
        this.upgradedNP = true;
    }

    protected final void setStar(int star) { setStar(star, 0); }

    protected final void setStar(int star, int starUpgrade) {
        this.baseStar = this.star = star;
        if (starUpgrade != 0) {
            this.upgradeStar = true;
            this.starUpgrade = starUpgrade;
        }
    }

    protected final void upgradeStar(int amount) {
        this.star = this.baseStar += amount;
        this.upgradedStar = true;
    }

    protected final void setCustomVar(String key, int base) {
        setCustomVar(key, base, 0);
    }

    protected final void setCustomVar(String key, int base, int upgrade) {
        setCustomVarValue(key, base, upgrade);

        if (!customVars.containsKey(key)) {
            QuickDynamicVariable var = new QuickDynamicVariable(key);
            customVars.put(key, var);
            BaseMod.addDynamicVariable(var);
            initializeDescription();
        }
    }

    protected enum VariableType {
        DAMAGE,
        BLOCK,
        MAGIC,
        NP,
        STAR
    }

    protected final void setCustomVar(String key, VariableType type, int base) {
        setCustomVar(key, type, base, 0);
    }

    protected final void setCustomVar(String key, VariableType type, int base, int upgrade) {
        setCustomVarValue(key, base, upgrade);

        switch (type) {
            case DAMAGE:
                calculateVarAsDamage(key);
                break;
            case BLOCK:
                calculateVarAsBlock(key);
                break;
            default:
                break;
        }

        if (!customVars.containsKey(key)) {
            QuickDynamicVariable var = new QuickDynamicVariable(key);
            customVars.put(key, var);
            BaseMod.addDynamicVariable(var);
            initializeDescription();
        }
    }

    protected final void setCustomVar(String key, VariableType type, int base,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> preCalc) {
        setCustomVar(key, type, base, 0, preCalc);
    }

    protected final void setCustomVar(String key, VariableType type, int base, int upgrade,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> preCalc) {
        setCustomVar(key, type, base, upgrade, preCalc, LocalVarInfo::noCalc);
    }

    protected final void setCustomVar(String key, VariableType type, int base,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> preCalc,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> postCalc) {
        setCustomVar(key, type, base, 0, preCalc, postCalc);
    }

    protected final void setCustomVar(String key, VariableType type, int base, int upgrade,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> preCalc,
            TriFunction<FGOCard, AbstractMonster, Integer, Integer> postCalc) {
        setCustomVarValue(key, base, upgrade);

        switch (type) {
            case DAMAGE:
                setVarCalculation(key, (c, m, baseVal) -> {
                    boolean wasMultiDamage = c.isMultiDamage;
                    c.isMultiDamage = false;

                    int origBase = c.baseDamage, origVal = c.damage;

                    c.baseDamage = preCalc.apply(c, m, baseVal);

                    if (m != null)
                        c.calculateCardDamage(m);
                    else
                        c.applyPowers();

                    c.damage = postCalc.apply(c, m, c.damage);

                    c.baseDamage = origBase;
                    c.isMultiDamage = wasMultiDamage;

                    int result = c.damage;
                    c.damage = origVal;

                    return result;
                });
                break;
            case BLOCK:
                setVarCalculation(key, (c, m, baseVal) -> {
                    int origBase = c.baseBlock, origVal = c.block;

                    c.baseBlock = preCalc.apply(c, m, baseVal);

                    if (m != null)
                        c.calculateCardDamage(m);
                    else
                        c.applyPowers();

                    c.block = postCalc.apply(c, m, c.block);

                    c.baseBlock = origBase;
                    int result = c.block;
                    c.block = origVal;
                    return result;
                });
                break;
            default:
                setVarCalculation(key, (c, m, baseVal) -> {
                    int tmp = baseVal;

                    tmp = preCalc.apply(c, m, tmp);
                    tmp = postCalc.apply(c, m, tmp);

                    return tmp;
                });
                break;
        }

        if (!customVars.containsKey(key)) {
            QuickDynamicVariable var = new QuickDynamicVariable(key);
            customVars.put(key, var);
            BaseMod.addDynamicVariable(var);
            initializeDescription();
        }
    }

    private void setCustomVarValue(String key, int base, int upg) {
        cardVariables.compute(key, (k, old) -> {
            if (old == null) {
                return new LocalVarInfo(base, upg);
            } else {
                old.base = base;
                old.upgrade = upg;
                return old;
            }
        });
    }

    protected final void colorCustomVar(String key, Color normalColor) {
        colorCustomVar(key, normalColor, Settings.GREEN_TEXT_COLOR, Settings.RED_TEXT_COLOR, Settings.GREEN_TEXT_COLOR);
    }

    protected final void colorCustomVar(String key, Color normalColor, Color increasedColor, Color decreasedColor) {
        colorCustomVar(key, normalColor, increasedColor, decreasedColor, increasedColor);
    }

    protected final void colorCustomVar(String key, Color normalColor, Color increasedColor, Color decreasedColor,
            Color upgradedColor) {
        LocalVarInfo var = getCustomVar(key);
        if (var == null) {
            throw new IllegalArgumentException("Attempted to set color of variable that hasn't been registered.");
        }

        var.normalColor = normalColor;
        var.increasedColor = increasedColor;
        var.decreasedColor = decreasedColor;
        var.upgradedColor = upgradedColor;
    }

    private LocalVarInfo getCustomVar(String key) {
        return cardVariables.get(key);
    }

    protected void calculateVarAsDamage(String key) {
        setVarCalculation(key, (c, m, base) -> {
            boolean wasMultiDamage = c.isMultiDamage;
            c.isMultiDamage = false;

            int origBase = c.baseDamage, origVal = c.damage;

            c.baseDamage = base;
            if (m != null)
                c.calculateCardDamage(m);
            else
                c.applyPowers();

            c.baseDamage = origBase;
            c.isMultiDamage = wasMultiDamage;

            int result = c.damage;
            c.damage = origVal;

            return result;
        });
    }

    protected void calculateVarAsBlock(String key) {
        setVarCalculation(key, (c, m, base) -> {
            int origBase = c.baseBlock, origVal = c.block;

            c.baseBlock = base;
            if (m != null)
                c.calculateCardDamage(m);
            else
                c.applyPowers();

            c.baseBlock = origBase;
            int result = c.block;
            c.block = origVal;
            return result;
        });
    }

    protected void setVarCalculation(String key, TriFunction<FGOCard, AbstractMonster, Integer, Integer> calculation) {
        cardVariables.get(key).calculation = calculation;
    }

    public int customVarBase(String key) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null)
            return -1;
        return var.base;
    }

    public int customVar(String key) {
        LocalVarInfo var = cardVariables == null ? null : cardVariables.get(key); // Prevents crashing when used with
                                                                                  // dynamic text
        if (var == null)
            return -1;
        return var.value;
    }

    public int[] customVarMulti(String key) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null)
            return null;
        return var.aoeValue;
    }

    public boolean isCustomVarModified(String key) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null)
            return false;
        return var.isModified();
    }

    public boolean customVarUpgraded(String key) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null)
            return false;
        return var.upgraded;
    }

    protected final void setCostUpgrade(int costUpgrade) {
        this.costUpgrade = costUpgrade;
        this.upgradeCost = true;
    }

    protected final void setExhaust() {
        setExhaust(true);
    }

    protected final void setEthereal() {
        setEthereal(true);
    }

    protected final void setInnate() {
        setInnate(true);
    }

    protected final void setSelfRetain() {
        setSelfRetain(true);
    }

    protected final void setExhaust(boolean exhaust) {
        setExhaust(exhaust, exhaust);
    }

    protected final void setEthereal(boolean ethereal) {
        setEthereal(ethereal, ethereal);
    }

    protected final void setInnate(boolean innate) {
        setInnate(innate, innate);
    }

    protected final void setSelfRetain(boolean retain) {
        setSelfRetain(retain, retain);
    }

    protected final void setExhaust(boolean baseExhaust, boolean upgExhaust) {
        this.baseExhaust = baseExhaust;
        this.upgExhaust = upgExhaust;
        this.exhaust = baseExhaust;
    }

    protected final void setEthereal(boolean baseEthereal, boolean upgEthereal) {
        this.baseEthereal = baseEthereal;
        this.upgEthereal = upgEthereal;
        this.isEthereal = baseEthereal;
    }

    protected void setInnate(boolean baseInnate, boolean upgInnate) {
        this.baseInnate = baseInnate;
        this.upgInnate = upgInnate;
        this.isInnate = baseInnate;
    }

    protected void setSelfRetain(boolean baseRetain, boolean upgRetain) {
        this.baseRetain = baseRetain;
        this.upgRetain = upgRetain;
        this.selfRetain = baseRetain;
    }

    @Override
    public AbstractCard makeStatEquivalentCopy() {
        AbstractCard candidate = super.makeStatEquivalentCopy();

        if (candidate instanceof FGOCard) {
            FGOCard card = (FGOCard) candidate;
            card.rawDescription = this.rawDescription;
            card.upgradesDescription = this.upgradesDescription;

            card.baseCost = this.baseCost;

            card.upgradeCost = this.upgradeCost;
            card.upgradeDamage = this.upgradeDamage;
            card.upgradeBlock = this.upgradeBlock;
            card.upgradeMagic = this.upgradeMagic;

            card.costUpgrade = this.costUpgrade;
            card.damageUpgrade = this.damageUpgrade;
            card.blockUpgrade = this.blockUpgrade;
            card.magicUpgrade = this.magicUpgrade;

            card.baseExhaust = this.baseExhaust;
            card.upgExhaust = this.upgExhaust;
            card.baseEthereal = this.baseEthereal;
            card.upgEthereal = this.upgEthereal;
            card.baseInnate = this.baseInnate;
            card.upgInnate = this.upgInnate;
            card.baseRetain = this.baseRetain;
            card.upgRetain = this.upgRetain;

            for (Map.Entry<String, LocalVarInfo> varEntry : cardVariables.entrySet()) {
                LocalVarInfo target = card.getCustomVar(varEntry.getKey()),
                        current = varEntry.getValue();
                if (target == null) {
                    card.setCustomVar(varEntry.getKey(), current.base, current.upgrade);
                    target = card.getCustomVar(varEntry.getKey());
                }
                target.base = current.base;
                target.value = current.value;
                target.aoeValue = current.aoeValue;
                target.upgrade = current.upgrade;
                target.calculation = current.calculation;
            }
        }
        
        if (candidate instanceof FGOCard) {
            FGOCard fgoCard = (FGOCard) candidate;
            fgoCard.np = this.np;
            fgoCard.star = this.star;
            fgoCard.baseNP = this.baseNP;
            fgoCard.baseStar = this.baseStar;
            fgoCard.upgradeNP = this.upgradeNP;
            fgoCard.upgradeStar = this.upgradeStar;
        }

        return candidate;
    }

    @Override
    public void upgrade() {
        if (upgraded) {
            return;
        }

        this.upgradeName();

        if (this.upgradesDescription) {
            if (cardStrings.UPGRADE_DESCRIPTION == null) {
                FGOMod.logger.error("Card " + cardID + " upgrades description and has null upgrade description.");
            } else {
                this.rawDescription = cardStrings.UPGRADE_DESCRIPTION;
            }
        }

        if (upgradeCost) {
            if (isCostModified && this.cost < this.baseCost && this.cost >= 0) {
                int diff = this.costUpgrade - this.baseCost; // how the upgrade alters cost
                this.upgradeBaseCost(this.cost + diff);
                if (this.cost < 0)
                    this.cost = 0;
            } else {
                upgradeBaseCost(costUpgrade);
            }
        }

        if (upgradeDamage)
            this.upgradeDamage(damageUpgrade);

        if (upgradeBlock)
            this.upgradeBlock(blockUpgrade);

        if (upgradeMagic)
            this.upgradeMagicNumber(magicUpgrade);
        
        if (upgradeNP) {
            upgradeNP(npUpgrade);
        }

        if (upgradeStar) {
            upgradeStar(starUpgrade);
        }

        for (LocalVarInfo var : cardVariables.values()) {
            upgradeCustomVar(var);
        }

        if (baseExhaust ^ upgExhaust)
            this.exhaust = upgExhaust;

        if (baseInnate ^ upgInnate)
            this.isInnate = upgInnate;

        if (baseEthereal ^ upgEthereal)
            this.isEthereal = upgEthereal;

        if (baseRetain ^ upgRetain)
            this.selfRetain = upgRetain;

        this.initializeDescription();
    }

    protected void upgradeCustomVar(String key) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null) {
            throw new NullPointerException("Custom variable with key " + key + " does not exist in " + this.getClass().getName());
        }
        upgradeCustomVar(var, var.upgrade);
    }

    protected void upgradeCustomVar(String key, int amount) {
        LocalVarInfo var = cardVariables.get(key);
        if (var == null) {
            throw new NullPointerException("Custom variable with key " + key + " does not exist in " + this.getClass().getName());
        }
        upgradeCustomVar(var, amount);
    }

    protected void upgradeCustomVar(LocalVarInfo var) {
        upgradeCustomVar(var, var.upgrade);
    }

    protected void upgradeCustomVar(LocalVarInfo var, int amt) {
        if (amt != 0) {
            var.base += amt;
            var.value = var.base;
            var.upgraded = true;
        }
    }

    boolean inCalc = false;

    @Override
    public void applyPowers() {
        if (!inCalc) {
            inCalc = true;
            for (LocalVarInfo var : cardVariables.values()) {
                var.value = var.calculation.apply(this, null, var.base);
            }
            if (isMultiDamage) {
                ArrayList<AbstractMonster> monsters = AbstractDungeon.getMonsters().monsters;
                AbstractMonster m;
                for (LocalVarInfo var : cardVariables.values()) {
                    if (var.aoeValue == null || var.aoeValue.length != monsters.size())
                        var.aoeValue = new int[monsters.size()];

                    for (int i = 0; i < monsters.size(); ++i) {
                        m = monsters.get(i);
                        var.aoeValue[i] = var.calculation.apply(this, m, var.base);
                    }
                }
            }
            inCalc = false;
        }

        super.applyPowers();
    }

    @Override
    public void calculateCardDamage(AbstractMonster m) {
        if (!inCalc) {
            inCalc = true;
            for (LocalVarInfo var : cardVariables.values()) {
                var.value = var.calculation.apply(this, m, var.base);
            }
            if (isMultiDamage) {
                ArrayList<AbstractMonster> monsters = AbstractDungeon.getMonsters().monsters;
                for (LocalVarInfo var : cardVariables.values()) {
                    if (var.aoeValue == null || var.aoeValue.length != monsters.size())
                        var.aoeValue = new int[monsters.size()];

                    for (int i = 0; i < monsters.size(); ++i) {
                        m = monsters.get(i);
                        var.aoeValue[i] = var.calculation.apply(this, m, var.base);
                    }
                }
            }
            inCalc = false;
        }

        super.calculateCardDamage(m);
    }

    @Override
    public void resetAttributes() {
        super.resetAttributes();

        for (LocalVarInfo var : cardVariables.values()) {
            var.value = var.base;
        }
    }

    private static class QuickDynamicVariable extends DynamicVariable {
        final String localKey, key;

        private FGOCard current = null;

        public QuickDynamicVariable(String key) {
            this.localKey = key;
            this.key = makeID(key);
        }

        @Override
        public String key() {
            return key;
        }

        @Override
        public void setIsModified(AbstractCard c, boolean v) {
            if (c instanceof FGOCard) {
                LocalVarInfo var = ((FGOCard) c).getCustomVar(localKey);
                if (var != null)
                    var.forceModified = v;
            }
        }

        @Override
        public boolean isModified(AbstractCard c) {
            return c instanceof FGOCard && (current = (FGOCard) c).isCustomVarModified(localKey);
        }

        @Override
        public int value(AbstractCard c) {
            return c instanceof FGOCard ? ((FGOCard) c).customVar(localKey) : 0;
        }

        @Override
        public int baseValue(AbstractCard c) {
            return c instanceof FGOCard ? ((FGOCard) c).customVarBase(localKey) : 0;
        }

        @Override
        public boolean upgraded(AbstractCard c) {
            return c instanceof FGOCard && ((FGOCard) c).customVarUpgraded(localKey);
        }

        @Override
        public Color getNormalColor() {
            LocalVarInfo var;
            if (current == null || (var = current.getCustomVar(localKey)) == null)
                return Settings.CREAM_COLOR;

            return var.normalColor;
        }

        @Override
        public Color getUpgradedColor() {
            LocalVarInfo var;
            if (current == null || (var = current.getCustomVar(localKey)) == null)
                return Settings.GREEN_TEXT_COLOR;

            return var.upgradedColor;
        }

        @Override
        public Color getIncreasedValueColor() {
            LocalVarInfo var;
            if (current == null || (var = current.getCustomVar(localKey)) == null)
                return Settings.GREEN_TEXT_COLOR;

            return var.increasedColor;
        }

        @Override
        public Color getDecreasedValueColor() {
            LocalVarInfo var;
            if (current == null || (var = current.getCustomVar(localKey)) == null)
                return Settings.RED_TEXT_COLOR;

            return var.decreasedColor;
        }
    }

    protected static class LocalVarInfo {
        int base, value, upgrade;
        int[] aoeValue = null;
        boolean upgraded = false;
        boolean forceModified = false;
        Color normalColor = Settings.CREAM_COLOR;
        Color upgradedColor = Settings.GREEN_TEXT_COLOR;
        Color increasedColor = Settings.GREEN_TEXT_COLOR;
        Color decreasedColor = Settings.RED_TEXT_COLOR;

        TriFunction<FGOCard, AbstractMonster, Integer, Integer> calculation = LocalVarInfo::noCalc;

        public LocalVarInfo(int base, int upgrade) {
            this.base = this.value = base;
            this.upgrade = upgrade;
        }

        private static int noCalc(FGOCard c, AbstractMonster m, int base) {
            return base;
        }

        public boolean isModified() {
            return forceModified || base != value;
        }
    }
}