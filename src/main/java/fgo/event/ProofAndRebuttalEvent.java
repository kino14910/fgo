package fgo.event;

import static fgo.FGOMod.eventPath;
import static fgo.FGOMod.makeID;
import static fgo.utils.ModHelper.eventAscension;
import static fgo.utils.RelicEventHelper.upgradeCards;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;
import com.megacrit.cardcrawl.vfx.combat.FlashAtkImgEffect;

import basemod.abstracts.events.PhasedEvent;
import basemod.abstracts.events.phases.TextPhase;
import fgo.cards.colorless.ProofAndRebuttal;

public class ProofAndRebuttalEvent extends PhasedEvent {
    public static final String ID = makeID(ProofAndRebuttalEvent.class.getSimpleName());
    private static final EventStrings eventStrings = CardCrawlGame.languagePack.getEventString(ID);
    private static final String[] DESCRIPTIONS = eventStrings.DESCRIPTIONS;
    private static final String[] OPTIONS = eventStrings.OPTIONS;
    private static final String NAME = eventStrings.NAME;
    private int goldLoss = eventAscension() 
            ? AbstractDungeon.miscRng.random(50, 75) 
            : AbstractDungeon.miscRng.random(40, 60);
    
    public ProofAndRebuttalEvent() {
        super(ID, NAME, eventPath("ProofAndRebuttalEvent"));
        AbstractPlayer p = AbstractDungeon.player;
        goldLoss = Math.min(goldLoss, p.gold);

        registerPhase(0, new TextPhase(DESCRIPTIONS[0])
            .addOption(new TextPhase.OptionInfo(OPTIONS[0], new ProofAndRebuttal())
                .setOptionResult(i -> {
                    AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new ProofAndRebuttal(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
                    transitionKey("Witness");
                }))
            .addOption(
                new TextPhase.OptionInfo(p.masterDeck.hasUpgradableCards() 
                    ? String.format(OPTIONS[1], goldLoss)
                    : OPTIONS[3]
                ).enabledCondition(p.masterDeck::hasUpgradableCards)
                .setOptionResult(i -> {
                    AbstractDungeon.effectList.add(new FlashAtkImgEffect(p.hb.cX, p.hb.cY, AbstractGameAction.AttackEffect.FIRE));
                    upgradeCards(2);
                    p.loseGold(goldLoss);
                    transitionKey("Enjoy");
                })
            )
            );

        registerPhase("Witness", new TextPhase(DESCRIPTIONS[1])
            .addOption(OPTIONS[2],  i -> openMap())
        );

        registerPhase("Enjoy", new TextPhase(DESCRIPTIONS[2])
            .addOption(OPTIONS[2],  i -> openMap())
        );

        transitionKey(0);
    }

    @Override
    public void onEnterRoom() {
        if (Settings.AMBIANCE_ON) {
            CardCrawlGame.sound.play("EVENT_SHINING");
        }
    }
}
