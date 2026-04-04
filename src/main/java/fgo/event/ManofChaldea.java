package fgo.event;

import static fgo.FGOMod.eventPath;
import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.EventStrings;
import com.megacrit.cardcrawl.vfx.RainingGoldEffect;

import basemod.abstracts.events.PhasedEvent;
import basemod.abstracts.events.phases.TextPhase;

public class ManofChaldea extends PhasedEvent {
    public static final String ID = makeID(ManofChaldea.class.getSimpleName());
    private static final EventStrings eventStrings = CardCrawlGame.languagePack.getEventString(ID);
    private static final String[] DESCRIPTIONS = eventStrings.DESCRIPTIONS;
    private static final String[] OPTIONS = eventStrings.OPTIONS;
    private static final String NAME = eventStrings.NAME;

    public ManofChaldea() {
        super(ID, NAME, eventPath("ManofChaldea"));
        
        // 人类即为过去延续到未来的足迹（记忆），只有一直积累经验、知识与故事，才能作为人而不断成长。 NL 一小时内有过的三两谈话。 NL 一天内交到的珍贵好友。 NL 一年内取得的耀眼成长。但是，没有人能够鲜明地记得经历的一切。 NL 留下的仅有结果。过程通常会被忘记。 NL 从长期角度，或是客观角度来看，少年与人们并无差别。
        registerPhase(0, new TextPhase(DESCRIPTIONS[0])
            .addOption(OPTIONS[0], i -> transitionKey("Phase 1")));

        // 每天结束时，他的记忆都会被刷新。 NL 化作一片空白。 NL 而少年，仅能保存其中重要的东西。23小时55分钟的丧失。每天经历过的耀眼记忆，都会被漂白。5分钟的信念。 NL 抵抗刷新的意志，让他获得了不会失去／无法忘怀的回忆。
        registerPhase("Phase 1", new TextPhase(DESCRIPTIONS[1])
            .addOption(OPTIONS[0], i -> transitionKey("Phase 2")));

        // 就这样，少年长大成人。不断积累着仅为人类（自己）所需的信息，到达了『一名人类的模样』。
        // 获得金币或恢复体力
        registerPhase("Phase 2", new TextPhase(DESCRIPTIONS[2])
            .addOption(OPTIONS[1], i -> {
                AbstractDungeon.effectList.add(new RainingGoldEffect(75));
                AbstractDungeon.player.gainGold(75);
                transitionKey("Gold");
            })
            .addOption(OPTIONS[2], i -> {
                AbstractDungeon.player.heal(15, true);
                transitionKey("Heal");
            }));
        
        registerPhase("Gold", new TextPhase(DESCRIPTIONS[3])
            .addOption(OPTIONS[3], i -> {
                AbstractDungeon.player.heal(15, true);
                openMap();
            }));
        
        registerPhase("Heal", new TextPhase(DESCRIPTIONS[4])
            .addOption(OPTIONS[3], i -> {
                AbstractDungeon.player.heal(15, true);
                openMap();
            }));
            
        transitionKey(0);
    }
}
