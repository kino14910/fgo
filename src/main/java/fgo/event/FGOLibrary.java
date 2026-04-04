
package fgo.event;

import static fgo.FGOMod.eventPath;
import static fgo.FGOMod.makeID;
import static fgo.utils.ModHelper.eventAscension;

import java.util.ArrayList;

import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import basemod.abstracts.events.PhasedEvent;
import basemod.abstracts.events.phases.TextPhase;
import fgo.cards.colorless.DazzlingMoon;
import fgo.cards.colorless.EightKindness;
import fgo.cards.colorless.MaraPapiyas;
import fgo.cards.colorless.PrimevalRune;
import fgo.cards.colorless.UndeadBird;

public class FGOLibrary extends PhasedEvent {
    public static final String ID = makeID(FGOLibrary.class.getSimpleName());
    private static final EventStrings eventStrings = CardCrawlGame.languagePack.getEventString(ID);
    private static final String[] DESCRIPTIONS = eventStrings.DESCRIPTIONS;
    private static final String[] OPTIONS = eventStrings.OPTIONS;
    private static final String NAME = eventStrings.NAME;
    private boolean pickCard = false;
    private final int healAmt;
    private final int maxHPAmt;
    private static final float healMultiplier = eventAscension() ? 0.2F : 0.33F;
    private static final float maxHPMultiplier = eventAscension() ? 0.15F : 0.1F;
    
    public FGOLibrary() {
        super(ID, NAME, eventPath("FGOLibrary"));

        AbstractCreature p = AbstractDungeon.player;
        healAmt = (int) (p.maxHealth * healMultiplier);
        maxHPAmt = (int) (p.maxHealth * maxHPMultiplier);

        registerPhase(0, new TextPhase(DESCRIPTIONS[0]) {
                @Override
                public void update() {
                    super.update();
                    ArrayList<AbstractCard> selectedCards = AbstractDungeon.gridSelectScreen.selectedCards;
                    if (pickCard && !AbstractDungeon.isScreenUp && !selectedCards.isEmpty()) {
                        AbstractCard c = selectedCards.get(0).makeCopy();
                        AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(c, Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
                        selectedCards.clear();
                    }
                }
            }
            .addOption(String.format(OPTIONS[0], maxHPAmt), i -> {
                pickCard = true;
                CardGroup group = new CardGroup(CardGroup.CardGroupType.UNSPECIFIED);
                group.addToBottom(new PrimevalRune());
                group.addToBottom(new DazzlingMoon());
                group.addToBottom(new EightKindness());
                group.addToBottom(new UndeadBird());
                group.addToBottom(new MaraPapiyas());

                AbstractDungeon.gridSelectScreen.open(group, 1, OPTIONS[3], false);
                AbstractDungeon.player.decreaseMaxHealth(maxHPAmt);
                transitionKey("Phase 2");
            })
            .addOption(String.format(OPTIONS[1], healAmt), i -> {
                p.heal(healAmt, true); 
                transitionKey("Phase 3");
            }));
            
        registerPhase("Phase 2", new TextPhase(DESCRIPTIONS[1])
            .addOption(OPTIONS[2], i -> openMap()));
        registerPhase("Phase 3", new TextPhase(DESCRIPTIONS[2])
            .addOption(OPTIONS[2], i -> openMap()));

        transitionKey(0);
    }
}
