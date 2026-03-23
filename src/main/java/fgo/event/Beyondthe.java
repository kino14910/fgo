package fgo.event;

import static fgo.FGOMod.eventPath;
import static fgo.FGOMod.makeID;
import static fgo.utils.ModHelper.eventAscension;

import com.badlogic.gdx.math.MathUtils;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import basemod.abstracts.events.PhasedEvent;
import basemod.abstracts.events.phases.TextPhase;
import basemod.abstracts.events.phases.TextPhase.OptionInfo;
import fgo.cards.colorless.Shvibzik;

public class Beyondthe extends PhasedEvent {
    public static final String ID = makeID(Beyondthe.class.getSimpleName());
    private static final EventStrings eventStrings = CardCrawlGame.languagePack.getEventString(ID);
    private static final String[] DESCRIPTIONS = eventStrings.DESCRIPTIONS;
    private static final String[] OPTIONS = eventStrings.OPTIONS;
    private static final String NAME = eventStrings.NAME;
    private static final int maxHPAmt = eventAscension() ? MathUtils.round(4) : MathUtils.round(6);
    public Beyondthe() {
        super(ID, NAME, eventPath("Beyondthe"));

        registerPhase(0, new TextPhase(DESCRIPTIONS[0])
            .addOption(OPTIONS[0], i -> transitionKey("Phase 1")));
        
        registerPhase("Phase 1", new TextPhase(DESCRIPTIONS[2])
            .addOption(new OptionInfo(OPTIONS[1], new Shvibzik())
                .setOptionResult(i -> {
                    AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new Shvibzik(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
                    transitionKey("Leave");
                }))
            .addOption(String.format(OPTIONS[2], maxHPAmt), i -> {
                AbstractDungeon.player.increaseMaxHp(maxHPAmt, true);
                transitionKey("Leave");
            })
        );

        registerPhase("Leave", new TextPhase(DESCRIPTIONS[3])
            .addOption(OPTIONS[3], i -> openMap()));

        transitionKey(0);
    }
}
