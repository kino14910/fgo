package fgo;

import static fgo.characters.CustomEnums.FGO_MASTER;
import static fgo.characters.Master.fgoNp;
import static fgo.utils.ModHelper.addToBot;
import static fgo.utils.ModHelper.getPowerCount;

import java.lang.reflect.Field;
import java.lang.reflect.Modifier;
import java.nio.charset.StandardCharsets;
import java.util.Arrays;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;
import java.util.Set;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import org.scannotation.AnnotationDB;

import com.badlogic.gdx.Files;
import com.badlogic.gdx.Gdx;
import com.badlogic.gdx.backends.lwjgl.LwjglFileHandle;
import com.badlogic.gdx.files.FileHandle;
import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.utils.GdxRuntimeException;
import com.evacipated.cardcrawl.modthespire.Loader;
import com.evacipated.cardcrawl.modthespire.ModInfo;
import com.evacipated.cardcrawl.modthespire.Patcher;
import com.evacipated.cardcrawl.modthespire.lib.SpireInitializer;
import com.google.gson.Gson;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.dungeons.Exordium;
import com.megacrit.cardcrawl.dungeons.TheBeyond;
import com.megacrit.cardcrawl.dungeons.TheCity;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.helpers.RelicLibrary;
import com.megacrit.cardcrawl.localization.CardStrings;
import com.megacrit.cardcrawl.localization.CharacterStrings;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.localization.OrbStrings;
import com.megacrit.cardcrawl.localization.PotionStrings;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.localization.RelicStrings;
import com.megacrit.cardcrawl.localization.TutorialStrings;
import com.megacrit.cardcrawl.localization.UIStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.monsters.MonsterGroup;
import com.megacrit.cardcrawl.rooms.AbstractRoom;
import com.megacrit.cardcrawl.ui.panels.EnergyPanel;
import com.megacrit.cardcrawl.unlock.UnlockTracker;

import basemod.AutoAdd;
import basemod.BaseMod;
import basemod.abstracts.CustomMultiPageFtue;
import basemod.eventUtil.AddEventParams;
import basemod.interfaces.AddAudioSubscriber;
import basemod.interfaces.EditCardsSubscriber;
import basemod.interfaces.EditCharactersSubscriber;
import basemod.interfaces.EditKeywordsSubscriber;
import basemod.interfaces.EditRelicsSubscriber;
import basemod.interfaces.EditStringsSubscriber;
import basemod.interfaces.OnCardUseSubscriber;
import basemod.interfaces.OnPlayerDamagedSubscriber;
import basemod.interfaces.OnStartBattleSubscriber;
import basemod.interfaces.PostBattleSubscriber;
import basemod.interfaces.PostCreateStartingDeckSubscriber;
import basemod.interfaces.PostInitializeSubscriber;
import basemod.interfaces.StartGameSubscriber;
import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.characters.Master;
import fgo.dynamicvariables.CriticalStarVariable;
import fgo.dynamicvariables.NoblePhantasmVariable;
import fgo.event.DevilSlot;
import fgo.event.FGOLibrary;
import fgo.event.ManofChaldea;
import fgo.event.ProofAndRebuttalEvent;
import fgo.monsters.Emiya;
import fgo.potions.BasePotion;
import fgo.powers.ArtsPerformancePower;
import fgo.powers.NPRatePower;
import fgo.relics.BaseRelic;
import fgo.relics.LockChocolateStrawberry;
import fgo.relics.SuitcaseFgo;
import fgo.ui.panelitems.NobleDeckPanelItem;
import fgo.ui.panels.CommandSpellPanel;
import fgo.ui.panels.FGOConfig;
import fgo.ui.panels.MasterSkin;
import fgo.ui.panels.NobleDeckCards;
import fgo.ui.screens.NobleDeckViewScreen;
import fgo.utils.GeneralUtils;
import fgo.utils.KeywordInfo;
import fgo.utils.Sounds;
import fgo.utils.TextureLoader;

@SpireInitializer
public class FGOMod implements
        EditCharactersSubscriber,
        EditCardsSubscriber, //up at the top
        EditStringsSubscriber,
        EditRelicsSubscriber,
        EditKeywordsSubscriber,
        AddAudioSubscriber,
        PostInitializeSubscriber,
        OnCardUseSubscriber,
        OnStartBattleSubscriber,
        OnPlayerDamagedSubscriber,
        PostBattleSubscriber,
        PostCreateStartingDeckSubscriber,
        StartGameSubscriber
        {
    public static ModInfo info;
    public static String modID; //Edit your pom.xml to change this
    public static FGOConfig config;
    static { loadModInfo(); }
    public static final Logger logger = LogManager.getLogger(modID); //Used to output to the console.
    private static final String resourcesFolder = checkResourcesPath();
    
    public static final int BASE_NP_PERCARD = 3;
    public static final int NP_RATE_POWER_MULTIPLIER = 2;
    
    static Texture[] ftues;
    static String[] tutTexts;
    //This is used to prefix the IDs of various objects like cards and relics,
    //to avoid conflicts between different mods using the same name for things.
    public static String makeID(String id) {
        return modID + ":" + id;
    }

    //This will be called by ModTheSpire because of the @SpireInitializer annotation at the top of the class.
    public static void initialize() {
        new FGOMod();
    }

    /*----------Create new Color----------*/
    public static final String CARD_ENERGY_ORB = uiPath("energyOrb");
    public static final String NOBLE_ENERGY_ORB = uiPath("energyOrb");
    public static final Color SILVER = CardHelper.getColor(200, 200, 200);
    public static final Color NOBLE = CardHelper.getColor(255, 215, 0);

    //默认卡牌背景
    private static final String DEFAULT_CC = imagePath("512/bg_master_s");
    private static final String DEFAULT_CC_PORTRAIT = imagePath("1024/bg_master");
    private static final String ENERGY_ORB_CC = imagePath("512/MASTEROrb");
    private static final String ENERGY_ORB_CC_PORTRAIT = imagePath("1024/MASTEROrb");
    //宝具牌背景
    private static final String NOBLE_ORB_CC = imagePath("512/noble_orb_512");
    private static final String NOBLE_ORB_CC_PORTRAIT = imagePath("1024/noble_orb_1024");
    private static final String ATTACK_Noble = imagePath("512/bg_empty_512");
    private static final String SKILL_Noble = imagePath("512/bg_empty_512");
    private static final String POWER_Noble = imagePath("512/bg_empty_512");
    //攻击、技能、能力牌的背景图片(1024)
    private static final String ATTACK_Noble_PORTRAIT = imagePath("1024/bg_empty_1024");
    private static final String SKILL_Noble_PORTRAIT = imagePath("1024/bg_empty_1024");
    private static final String POWER_Noble_PORTRAIT = imagePath("1024/bg_empty_1024");
    //角色图标。
    private static final String MY_CHARACTER_BUTTON = imagePath("charSelect/MasterButton");
    //默认背景图片。
    private static final String MASTER_PORTRAIT = imagePath("charSelect/MasterPortrait1");
    

    public FGOMod() {
        BaseMod.subscribe(this); //This will make BaseMod trigger all the subscribers at their appropriate times.
        BaseMod.addColor(FGOCardColor.FGO, SILVER, DEFAULT_CC, DEFAULT_CC, DEFAULT_CC, ENERGY_ORB_CC, DEFAULT_CC_PORTRAIT, DEFAULT_CC_PORTRAIT, DEFAULT_CC_PORTRAIT, ENERGY_ORB_CC_PORTRAIT, CARD_ENERGY_ORB);
        BaseMod.addColor(FGOCardColor.NOBLE_PHANTASM, NOBLE, ATTACK_Noble, SKILL_Noble, POWER_Noble, NOBLE_ORB_CC, ATTACK_Noble_PORTRAIT, SKILL_Noble_PORTRAIT, POWER_Noble_PORTRAIT, NOBLE_ORB_CC_PORTRAIT, NOBLE_ENERGY_ORB);
        BaseMod.addColor(FGOCardColor.FGO_DERIVATIVE, SILVER, DEFAULT_CC, DEFAULT_CC, DEFAULT_CC, ENERGY_ORB_CC, DEFAULT_CC_PORTRAIT, DEFAULT_CC_PORTRAIT, DEFAULT_CC_PORTRAIT, ENERGY_ORB_CC_PORTRAIT, CARD_ENERGY_ORB);
        BaseMod.addSaveField(makeID("commandSpellCount"), new CommandSpellPanel());
        BaseMod.addSaveField(makeID("modifierIndexes"), new MasterSkin());
        BaseMod.addSaveField(makeID("nobleDeckCards"), new NobleDeckCards());
        logger.info(modID + " subscribed to BaseMod.");
    }

    private boolean shouldRenderNobleDeck = false;
    @Override
    public void receivePostInitialize() {
        registerPotions();
        registerEvents();
        //This loads the image used as an icon in the in-game mods menu.
        Texture badgeTexture = TextureLoader.getTexture(uiPath("badge"));
        //Set up the mod information displayed in the in-game mods menu.
        //The information used is taken from your pom.xml file.

        //If you want to set up a config panel, that will be done here.
        //The Mod Badges page has a basic example of this, but setting up config is overall a bit complex.

        // this has been loaded from receiveEditCards
        // config = new FGOConfig();

        BaseMod.registerModBadge(badgeTexture, info.Name, GeneralUtils.arrToString(info.Authors), info.Description, config);

        if(FGOConfig.enableEmiya){
            BaseMod.addMonster(Emiya.ID, Emiya.NAME, () -> new MonsterGroup(new AbstractMonster[]{new Emiya()}));
            BaseMod.addBoss(TheBeyond.ID, Emiya.ID, monsterPath("map_emiya"), monsterPath("map_emiya_outline"));
        }
        shouldRenderNobleDeck = true;
    }

            /*----------Localization----------*/

    //This is used to load the appropriate localization files based on language.
    private static String getLangString()
    {
        return Settings.language.name().toLowerCase();
    }
    private static final String defaultLanguage = "eng";

    public static final Map<String, KeywordInfo> keywords = new HashMap<>();

    @Override
    public void receiveEditStrings() {
        /*
            First, load the default localization.
            Then, if the current language is different, attempt to load localization for that language.
            This results in the default localization being used for anything that might be missing.
            The same process is used to load keywords slightly below.
        */
        loadLocalization(defaultLanguage); //no exception catching for default localization; you better have at least one that works.
        if (!defaultLanguage.equals(getLangString())) {
            try {
                loadLocalization(getLangString());
            }
            catch (GdxRuntimeException e) {
                e.printStackTrace();
            }
        }
    }

    @Override
    public void receiveAddAudio() {
        loadAudio(Sounds.class);
    }

    private static final String[] AUDIO_EXTENSIONS = { ".ogg", ".wav", ".mp3" }; //There are more valid types, but not really worth checking them all here
    private void loadAudio(Class<?> cls) {
        try {
            Field[] fields = cls.getDeclaredFields();
            outer:
            for (Field f : fields) {
                int modifiers = f.getModifiers();
                if (Modifier.isStatic(modifiers) && Modifier.isPublic(modifiers) && f.getType().equals(String.class)) {
                    String s = (String) f.get(null);
                    if (s == null) { //If no defined value, determine path using field name
                        s = audioPath(f.getName());

                        for (String ext : AUDIO_EXTENSIONS) {
                            String testPath = s + ext;
                            if (Gdx.files.internal(testPath).exists()) {
                                s = testPath;
                                BaseMod.addAudio(s, s);
                                f.set(null, s);
                                continue outer;
                            }
                        }
                        throw new Exception("Failed to find an audio file \"" + f.getName() + "\" in " + resourcesFolder + "/audio; check to ensure the capitalization and filename are correct.");
                    }
                    else { //Otherwise, load defined path
                        if (Gdx.files.internal(s).exists()) {
                            BaseMod.addAudio(s, s);
                        }
                        else {
                            throw new Exception("Failed to find audio file \"" + s + "\"; check to ensure this is the correct filepath.");
                        }
                    }
                }
            }
        }
        catch (Exception e) {
            logger.error("Exception occurred in loadAudio: ", e);
        }
    }

    private void loadLocalization(String lang) {
        //While this does load every type of localization, most of these files are just outlines so that you can see how they're formatted.
        //Feel free to comment out/delete any that you don't end up using.
        BaseMod.loadCustomStringsFile(CardStrings.class,
                localizationPath(lang, "CardStrings.json"));
        BaseMod.loadCustomStringsFile(CharacterStrings.class,
                localizationPath(lang, "CharacterStrings.json"));
        BaseMod.loadCustomStringsFile(EventStrings.class,
                localizationPath(lang, "EventStrings.json"));
        BaseMod.loadCustomStringsFile(OrbStrings.class,
                localizationPath(lang, "OrbStrings.json"));
        BaseMod.loadCustomStringsFile(PotionStrings.class,
                localizationPath(lang, "PotionStrings.json"));
        BaseMod.loadCustomStringsFile(PowerStrings.class,
                localizationPath(lang, "PowerStrings.json"));
        BaseMod.loadCustomStringsFile(RelicStrings.class,
                localizationPath(lang, "RelicStrings.json"));
        BaseMod.loadCustomStringsFile(UIStrings.class,
                localizationPath(lang, "UIStrings.json"));
        BaseMod.loadCustomStringsFile(MonsterStrings.class,
                localizationPath(lang, "MonsterStrings.json"));
        BaseMod.loadCustomStringsFile(TutorialStrings.class,
                localizationPath(lang, "TutorialStrings.json"));
    }

    @Override
    public void receiveEditKeywords()
    {
        Gson gson = new Gson();
        String json = Gdx.files.internal(localizationPath(defaultLanguage, "Keywords.json")).readString(String.valueOf(StandardCharsets.UTF_8));
        KeywordInfo[] keywords = gson.fromJson(json, KeywordInfo[].class);
        for (KeywordInfo keyword : keywords) {
            keyword.prep();
            registerKeyword(keyword);
        }

        if (!defaultLanguage.equals(getLangString())) {
            try
            {
                json = Gdx.files.internal(localizationPath(getLangString(), "Keywords.json")).readString(String.valueOf(StandardCharsets.UTF_8));
                keywords = gson.fromJson(json, KeywordInfo[].class);
                for (KeywordInfo keyword : keywords) {
                    keyword.prep();
                    registerKeyword(keyword);
                }
            }
            catch (Exception e)
            {
                logger.warn(modID + " does not support " + getLangString() + " keywords.");
            }
        }
    }

    private void registerKeyword(KeywordInfo info) {
        BaseMod.addKeyword(modID.toLowerCase(), info.PROPER_NAME, info.NAMES, info.DESCRIPTION, info.COLOR);
        if (!info.ID.isEmpty())
        {
            keywords.put(info.ID, info);
        }
    }

    //These methods are used to generate the correct filepaths to various parts of the resources folder.
    public static String localizationPath(String lang, String file) {
        return resourcesFolder + "/localization/" + lang + "/" + file;
    }

    public static String audioPath(String file) {
        return resourcesFolder + "/audio/" + file;
    }
    public static String musicPath(String file) {
        return audioPath("music/" + file);
    }
    public static String soundPath(String file) {
        return audioPath("sound/" + file);
    }
    public static String imagePath(String file) {
        return resourcesFolder + "/images/" + file + ".png";
    }
    public static String characterPath(String file) {
        return imagePath("character/" + file);
    }
    public static String monsterPath(String file) {
        return imagePath("monsters/" + file);
    }
    public static String powerPath(String file) {
        return imagePath("powers/" + file);
    }
    public static String relicPath(String file) {
        return imagePath("relics/" + file);
    }
    public static String uiPath(String file) {
        return imagePath("ui/" + file);
    }
    public static String vfxPath(String file) {
        return imagePath("vfx/" + file);
    }
    public static String cardPath(String file) {
        return imagePath("cards/" + file);
    }
    public static String eventPath(String file) {
        return imagePath("events/" + file);
    }

    /**
     * Checks the expected resources path based on the package name.
     */
    private static String checkResourcesPath() {
        String name = FGOMod.class.getName(); //getPackage can be iffy with patching, so class name is used instead.
        int separator = name.indexOf('.');
        if (separator > 0)
            name = name.substring(0, separator);

        FileHandle resources = new LwjglFileHandle(name, Files.FileType.Internal);

        if (!resources.exists()) {
            throw new RuntimeException("\n\tFailed to find resources folder; expected it to be named \"" + name + "\"." +
                    " Either make sure the folder under resources has the same name as your mod's package, or change the line\n" +
                    "\t\"private static final String resourcesFolder = checkResourcesPath();\"\n" +
                    "\tat the top of the " + FGOMod.class.getSimpleName() + " java file.");
        }
        if (!resources.child("images").exists()) {
            throw new RuntimeException("\n\tFailed to find the 'images' folder in the mod's 'resources/" + name + "' folder; Make sure the " +
                    "images folder is in the correct location.");
        }
        if (!resources.child("localization").exists()) {
            throw new RuntimeException("\n\tFailed to find the 'localization' folder in the mod's 'resources/" + name + "' folder; Make sure the " +
                    "localization folder is in the correct location.");
        }

        return name;
    }

    /**
     * This determines the mod's ID based on information stored by ModTheSpire.
     */
    private static void loadModInfo() {
        Optional<ModInfo> infos = Arrays.stream(Loader.MODINFOS).filter((modInfo)->{
            AnnotationDB annotationDB = Patcher.annotationDBMap.get(modInfo.jarURL);
            if (annotationDB == null)
                return false;
            Set<String> initializers = annotationDB.getAnnotationIndex().getOrDefault(SpireInitializer.class.getName(), Collections.emptySet());
            return initializers.contains(FGOMod.class.getName());
        }).findFirst();
        if (infos.isPresent()) {
            info = infos.get();
            modID = info.ID;
        }
        else {
            throw new RuntimeException("Failed to determine mod info/ID based on initializer.");
        }
    }

    @Override
    public void receiveEditCharacters() {
        BaseMod.addCharacter(new Master("Master"), MY_CHARACTER_BUTTON, MASTER_PORTRAIT, FGO_MASTER);
    }

    @Override
    public void receiveEditCards() { //somewhere in the class
        if (config == null) {
            config = new FGOConfig();
        }
        AutoAdd autoAdd = new AutoAdd(modID);
        //Loads files from this mod
        autoAdd.packageFilter(FGOCard.class) //In the same package as this class
                .notPackageFilter("fgo.cards.optionCards")
                .notPackageFilter("fgo.cards.deprecated");

        if (!FGOConfig.enableColorlessCards) {
            autoAdd = autoAdd.notPackageFilter("fgo.cards.colorless");
        }

        autoAdd.setDefaultSeen(true) //And marks them as seen in the compendium
                .cards(); //Adds the cards
        BaseMod.addDynamicVariable(new NoblePhantasmVariable());
        BaseMod.addDynamicVariable(new CriticalStarVariable());
    }

    @Override
    public void receiveEditRelics() {
        new AutoAdd(modID)
                .packageFilter(BaseRelic.class)
                .notPackageFilter("fgo.relics.deprecated")
                .any(BaseRelic.class, (info, relic) -> { //Run this code for any classes that extend this class
                    if (relic.pool != null)
                        BaseMod.addRelicToCustomPool(relic, relic.pool); //Register a custom character specific relic
                    else
                        BaseMod.addRelic(relic, relic.relicType); //Register a shared or base game character specific relic

                    //If the class is annotated with @AutoAdd.Seen, it will be marked as seen, making it visible in the relic library.
                    //If you want all your relics to be visible by default, just remove this if statement.
    //                if (info.seen)
                        UnlockTracker.markRelicAsSeen(relic.relicId);
                });
    }

     public static void registerPotions() {
        new AutoAdd(modID) //Loads files from this mod
            .packageFilter(BasePotion.class) //In the same package as this class
            .any(BasePotion.class, (info, potion) -> { //Run this code for any classes that extend this class
                //These three null parameters are colors.
                //If they're not null, they'll overwrite whatever color is set in the potions themselves.
                //This is an old feature added before having potions determine their own color was possible.
                BaseMod.addPotion(potion.getClass(), BaseMod.getPotionLiquidColor(potion.ID), BaseMod.getPotionHybridColor(potion.ID), BaseMod.getPotionSpotsColor(potion.ID), potion.ID, FGO_MASTER);
                //playerClass will make a potion character-specific. By default, it's null and will do nothing.
            });
    }

    public static void registerEvents() {
        //事件。
        // BaseMod.addEvent(WinterEvent.ID, WinterEvent.class, TheCity.ID);
        // BaseMod.addEvent("FGOLibrary", FGOLibrary.class, "TheCity");
        // BaseMod.addEvent(ConflictEvent.ID, ConflictEvent.class, TheBeyond.ID);
        BaseMod.addEvent(ProofAndRebuttalEvent.ID, ProofAndRebuttalEvent.class, Exordium.ID);
        BaseMod.addEvent(ManofChaldea.ID, ManofChaldea.class, TheBeyond.ID);
        BaseMod.addEvent(DevilSlot.ID, DevilSlot.class, TheBeyond.ID);
        BaseMod.addEvent((new AddEventParams.Builder(FGOLibrary.ID, FGOLibrary.class))
                .dungeonID(TheCity.ID)
                .playerClass(FGO_MASTER)
                .create());
    }
    @Override
    public void receiveCardUsed(AbstractCard card) {
        if (!isMaster() || card == null || card.color == FGOCardColor.NOBLE_PHANTASM) {
            return;
        }

        int npMultiplier = calculateNpRateMultiplier();
        int npGain = calculateNpGain(card, npMultiplier);

        if (npGain > 0) {
            addToBot(new FgoNpAction(npGain));
        }
    }

    @Override
    public void receiveOnBattleStart(AbstractRoom abstractRoom) {
        if (!isMaster()) {
            return;
        }
        
        fgoNp = 0;
        if (AbstractDungeon.player.hasRelic(SuitcaseFgo.ID)) {
            fgoNp = 20;
        }
        if(AbstractDungeon.floorNum == 16) {
            AbstractDungeon.getCurrRoom().spawnRelicAndObtain(Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f, RelicLibrary.getRelic(LockChocolateStrawberry.ID).makeCopy());
        }
        
        if (FGOConfig.enableFtue && AbstractDungeon.getCurrRoom().monsters != null){
            ftues = new Texture[] {
                    ImageMaster.loadImage(uiPath("tutorial/1")),
                    ImageMaster.loadImage(uiPath("tutorial/2")),
                    ImageMaster.loadImage(uiPath("tutorial/3")),
                    ImageMaster.loadImage(uiPath("tutorial/4")),
                };
                tutTexts = CardCrawlGame.languagePack.getUIString(makeID("ftue")).TEXT;
                tutTexts[0] = String.format(tutTexts[0], BASE_NP_PERCARD);    
            boolean hasTexture  = false;
            for (Texture tex : ftues) {
                if (tex == null) {
                    continue;
                }
                hasTexture = true;
                System.out.println("ftue: " + tex.toString());
            }
            System.out.println("hasTexture: " + hasTexture);
            AbstractDungeon.ftue = new CustomMultiPageFtue(ftues, tutTexts);
            FGOConfig.enableFtue = false;
            FGOMod.config.save();
        }
    }

    @Override
    public void receivePostBattle(AbstractRoom r) {
        if (isMaster()) {
            //在每场战斗开始时宝具值变为0。
            addToBot(new FgoNpAction(-300));
        }
    }

    @Override
    public int receiveOnPlayerDamaged(int damage, DamageInfo damageInfo) {
        if (!isMaster()) {
            return damage;
        }

        if (damageInfo.type != DamageInfo.DamageType.NORMAL
                || damageInfo.owner == null
                || damageInfo.owner == AbstractDungeon.player
                || AbstractDungeon.currMapNode == null
                || AbstractDungeon.getCurrRoom().phase != AbstractRoom.RoomPhase.COMBAT
                || damage == 99999) {
            return damage;
        }

        addToBot(new FgoNpAction(AbstractDungeon.player.hasPower(NPRatePower.POWER_ID) ? 2 * damage : damage));
        return damage;
    }

    @Override
    public void receivePostCreateStartingDeck(AbstractPlayer.PlayerClass playerClass, CardGroup cardGroup) {
        if (playerClass == FGO_MASTER) {
            CommandSpellPanel.reset();
            NobleDeckCards.reset();
        }
    }

    @Override
    public void receiveStartGame() {
        if (isMaster()) {
            NobleDeckCards.getNobleCards().clear();
            NobleDeckCards.addCards(NobleDeckCards.cards);
            if (shouldRenderNobleDeck) {
                BaseMod.addTopPanelItem(new NobleDeckPanelItem());
                BaseMod.addCustomScreen(new NobleDeckViewScreen());
                shouldRenderNobleDeck = false;
            }
        }
    }

    
    private int calculateNpRateMultiplier() {
        int multiplier = BASE_NP_PERCARD;
        
        AbstractPlayer p = AbstractDungeon.player;
        if (p.hasPower(NPRatePower.POWER_ID)) {
            multiplier *= NP_RATE_POWER_MULTIPLIER;
        }

        multiplier += getPowerCount(p, ArtsPerformancePower.POWER_ID);
        
        return multiplier;
    }

    private int calculateNpGain(AbstractCard card, int npRateMultiplier) {
        int costForTurn = card.costForTurn;
        
        if (costForTurn == -1) {
            return EnergyPanel.totalCount * npRateMultiplier;
        }
        
        return costForTurn > 0 ? costForTurn * npRateMultiplier : 0;
    }

    private boolean isMaster() {
        return AbstractDungeon.player instanceof Master;
    }

}
