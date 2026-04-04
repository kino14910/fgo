package fgo.event;

import static fgo.FGOMod.eventPath;
import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.vfx.cardManip.PurgeCardEffect;

import basemod.abstracts.events.PhasedEvent;
import basemod.abstracts.events.phases.TextPhase;
import fgo.relics.BB;

public class DevilSlot extends PhasedEvent {
    public static final String ID = makeID(DevilSlot.class.getSimpleName());
    private static final EventStrings eventStrings = CardCrawlGame.languagePack.getEventString(ID);
    private static final String[] DESCRIPTIONS = eventStrings.DESCRIPTIONS;
    private static final String[] OPTIONS = eventStrings.OPTIONS;
    private static final String NAME = eventStrings.NAME;

    public DevilSlot() {
        super(ID, NAME, eventPath("DevilSlot"));
        
        // 这可是最糟的情况，你走投无路。
        registerPhase(0, new TextPhase(DESCRIPTIONS[0])
            .addOption(OPTIONS[0], i -> {
                AbstractDungeon.getCurrRoom().spawnRelicAndObtain(Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f, new BB());
                transitionKey("BB");
            })
            .addOption(OPTIONS[1], i -> {
                CardGroup group = CardGroup.getGroupWithoutBottledCards(AbstractDungeon.player.masterDeck.getPurgeableCards());
                if (!group.isEmpty()) {
                    imageEventText.updateBodyText(DESCRIPTIONS[2]);
                    AbstractDungeon.gridSelectScreen.open(group, 1, OPTIONS[2], false);
                }
                transitionKey("Leave");
            })
        );

        // 好巧不巧。
        registerPhase("BB", new TextPhase(DESCRIPTIONS[1])
            .addOption(OPTIONS[2], i -> openMap()));

        registerPhase("Leave", new TextPhase(DESCRIPTIONS[2])
            .addOption(OPTIONS[2], i -> openMap()));

        transitionKey(0);
    }

    @Override
    public void update() {
        super.update();
        if (!AbstractDungeon.gridSelectScreen.selectedCards.isEmpty()) {
            AbstractCard c = AbstractDungeon.gridSelectScreen.selectedCards.get(0);
            AbstractDungeon.effectList.add(new PurgeCardEffect(c));
            AbstractDungeon.player.masterDeck.removeCard(c);
            AbstractDungeon.gridSelectScreen.selectedCards.remove(c);
        }
    }
}
