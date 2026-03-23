package fgo.utils;

import static fgo.FGOMod.makeID;

import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.List;
import java.util.function.Predicate;
import java.util.stream.Collectors;

import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.badlogic.gdx.math.MathUtils;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.CardGroup;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.monsters.MonsterGroup;
import com.megacrit.cardcrawl.powers.AbstractPower;
import com.megacrit.cardcrawl.random.Random;
import com.megacrit.cardcrawl.vfx.AbstractGameEffect;

import basemod.BaseMod;
import basemod.ReflectionHacks;

public class ModHelper {
    public static void addToBot(AbstractGameAction action) {
        AbstractDungeon.actionManager.addToBottom(action);
    }

    public static void addToTop(AbstractGameAction action) {
        AbstractDungeon.actionManager.addToTop(action);
    }

    public static void addToBotAbstract(Lambda func) {
        AbstractDungeon.actionManager.addToBottom(new AbstractGameAction() {
            @Override
            public void update() {
                func.run();
                isDone = true;
            }
        });
    }

    public static void addToTopAbstract(Lambda func) {
        AbstractDungeon.actionManager.addToTop(new AbstractGameAction() {
            @Override
            public void update() {
                func.run();
                isDone = true;
            }
        });
    }
    
    public static void addEffectAbstract(Lambda func) {
        addEffectAbstract(func, true);
    }

    public static void addEffectAbstract(Lambda func, boolean topLevel) {
        AbstractGameEffect effect = new AbstractGameEffect() {
            @Override
            public void update() {
                func.run();
                isDone = true;
            }

            @Override
            public void render(SpriteBatch spriteBatch) {
            }

            @Override
            public void dispose() {
            }
        };
        
        if (topLevel) {
            AbstractDungeon.topLevelEffectsQueue.add(effect);
        } else {
            AbstractDungeon.effectsQueue.add(effect);
        }
    }

    public interface Lambda extends Runnable {
    }

    public static int getPowerCount(AbstractCreature creature, String powerID) {
        return creature.hasPower(powerID) ? creature.getPower(powerID).amount : 0;
    }

    public static List<FindResult> findCardsInGroup(Predicate<AbstractCard> predicate, CardGroup group) {
        List<FindResult> result = new ArrayList<>();
        for (AbstractCard card : group.group) {
            if (predicate.test(card)) {
                FindResult findResult = new FindResult();
                findResult.card = card;
                findResult.group = group;
                result.add(findResult);
            }
        }
        return result;
    }

    public static List<FindResult> findCards(Predicate<AbstractCard> predicate, boolean hand, boolean discard, boolean draw, boolean exhaust, boolean limbo) {
        List<FindResult> result = new ArrayList<>();
        if (hand) result.addAll(findCardsInGroup(predicate, AbstractDungeon.player.hand));
        if (discard) result.addAll(findCardsInGroup(predicate, AbstractDungeon.player.discardPile));
        if (draw) result.addAll(findCardsInGroup(predicate, AbstractDungeon.player.drawPile));
        if (exhaust) result.addAll(findCardsInGroup(predicate, AbstractDungeon.player.exhaustPile));
        if (limbo) result.addAll(findCardsInGroup(predicate, AbstractDungeon.player.limbo));
        return result;
    }

    public static List<FindResult> findCards(Predicate<AbstractCard> predicate, boolean shuffle) {
        List<FindResult> result = findCards(predicate);
        if (shuffle) {
            Collections.shuffle(result, new java.util.Random(AbstractDungeon.cardRandomRng.randomLong()));
        }
        return result;
    }

    public static List<FindResult> findCards(Predicate<AbstractCard> predicate) {
        return findCards(predicate, true, true, true, true, false);
    }

    public static class FindResult {
        public AbstractCard card;
        public CardGroup group;
    }

    public static AbstractMonster betterGetRandomMonster() {
        return getRandomMonster(m -> !(m.isDying || m.isEscaping || m.halfDead || m.currentHealth <= 0), true);
    }
    
    public static boolean check(AbstractCreature m) {
        return !(m == null || m.isDying || m.isEscaping || m.halfDead || m.currentHealth <= 0);
    }

    public static AbstractMonster getRandomMonster(Predicate<AbstractMonster> predicate, boolean aliveOnly) {
        MonsterGroup group = AbstractDungeon.getCurrRoom().monsters;
        Random rng = AbstractDungeon.cardRandomRng;
        if (group.areMonstersBasicallyDead()) {
            return null;
        }

        List<AbstractMonster> tmp;
        if (predicate == null) {
            if (aliveOnly) {
                tmp = group.monsters.stream()
                    .filter(m -> check(m))
                    .collect(Collectors.toList());

                if (tmp.isEmpty()) {
                    return null;
                }
                return tmp.get(rng.random(0, tmp.size() - 1));
            }
            return group.monsters.get(rng.random(0, group.monsters.size() - 1));
        }

        if (group.monsters.size() == 1) {
            AbstractMonster m = group.monsters.get(0);
            return predicate.test(m) ? m : null;
        }

        tmp = group.monsters.stream()
            .filter(m -> !aliveOnly || (!m.halfDead && !m.isDying && !m.isEscaping))
            .filter(predicate::test)
            .collect(Collectors.toList());

        if (tmp.isEmpty()) {
            return null;
        }
        return tmp.get(rng.random(tmp.size() - 1));

    }
    
    public static AbstractMonster getMonsterWithMaxHealth() {
        if (AbstractDungeon.currMapNode == null
                || AbstractDungeon.getMonsters() == null
                || AbstractDungeon.getMonsters().monsters == null) {
            return null;
        }
        return AbstractDungeon.getMonsters().monsters.stream()
                .filter(ModHelper::check)
                .max(Comparator.comparingInt(m -> m.maxHealth))
                .orElse(null);
    }

    public static boolean hasRelic(String relicID) {
        return AbstractDungeon.player != null && AbstractDungeon.player.hasRelic(makeID(relicID));
    }

    public static void applyEnemyPowersOnly(DamageInfo info, AbstractCreature target, boolean reset) {
        if (reset) {
            info.output = info.base;
            info.isModified = false;
        }
        float tmp = (float) info.output;

        for (AbstractPower p : target.powers) {
            tmp = p.atDamageReceive(tmp, info.type);
            if (info.base != info.output) {
                info.isModified = true;
            }
        }

        for (AbstractPower p : target.powers) {
            tmp = p.atDamageFinalReceive(tmp, info.type);
            if (info.base != info.output) {
                info.isModified = true;
            }
        }

        if (tmp < 0.0f) {
            tmp = 0.0f;
        }

        info.output = MathUtils.floor(tmp);
    }

    public static boolean moreDamageAscension(AbstractMonster.EnemyType type) {
        int level = 2;
        switch (type) {
            case NORMAL:
                level = 2;
                break;
            case ELITE:
                level = 3;
                break;
            case BOSS:
                level = 4;
                break;
        }
        return AbstractDungeon.ascensionLevel >= level;
    }

    public static boolean moreHPAscension(AbstractMonster.EnemyType type) {
        int level = 7;
        switch (type) {
            case NORMAL:
                level = 7;
                break;
            case ELITE:
                level = 8;
                break;
            case BOSS:
                level = 9;
                break;
        }
        return AbstractDungeon.ascensionLevel >= level;
    }

    public static boolean specialAscension(AbstractMonster.EnemyType type) {
        int level = 17;
        switch (type) {
            case NORMAL:
                level = 17;
                break;
            case ELITE:
                level = 18;
                break;
            case BOSS:
                level = 19;
                break;
        }
        return AbstractDungeon.ascensionLevel >= level;
    }
    
    public static boolean eventAscension() {
        return AbstractDungeon.ascensionLevel >= 15;
    }

    public static boolean hasBuff(AbstractCreature creature, AbstractPower.PowerType type) {
        return creature.powers.stream().anyMatch(power -> power.type == type);
    }
    
    public static boolean isMintySpireTIDEnabled() {
        try {
            if (BaseMod.hasModID("mintyspire:")) {
                Class<?> mintySpireClass = Class.forName("mintySpire.MintySpire");
                java.lang.reflect.Method showTIDMethod = ReflectionHacks.getCachedMethod(mintySpireClass, "showTID");
                if (showTIDMethod != null) {
                    return (boolean) showTIDMethod.invoke(null);
                }
            }
        } catch (Exception e) {
            // mintySpire not found or error accessing config
        }
        return false;
    }
}
