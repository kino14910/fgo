package fgo.monsters;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Consumer;
import java.util.function.Supplier;

import com.badlogic.gdx.math.MathUtils;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePostfixPatch;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateFastAttackAction;
import com.megacrit.cardcrawl.actions.animations.AnimateHopAction;
import com.megacrit.cardcrawl.actions.animations.AnimateJumpAction;
import com.megacrit.cardcrawl.actions.animations.AnimateSlowAttackAction;
import com.megacrit.cardcrawl.actions.animations.ShoutAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.RollMoveAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.monsters.EnemyMoveInfo;

import basemod.abstracts.CustomMonster;
import fgo.FGOMod;
import fgo.utils.ModHelper;
import fgo.vfx.MoveToEffect;

public abstract class BaseMonster extends CustomMonster {
    public MonsterStrings monsterStrings;
    public String NAME;
    public String[] MOVES;
    public String[] DIALOG;
    public int turnCount;
    public String bgm;
    public AbstractPlayer p = AbstractDungeon.player;
    public List<MoveInfo> moveInfos = new ArrayList<>();
    public EnemyMoveInfo currMove;
    public boolean moreDamageAs, moreHPAs, specialAs;
    public float floatIndex = 0;
    public Consumer<BaseMonster> preBattleAction;

    public BaseMonster(String name, String id, int maxHealth, float hb_x, float hb_y, float hb_w, float hb_h, String imgUrl, float x, float y) {
        super(name, id, maxHealth, hb_x, hb_y, hb_w, hb_h, imgUrl, x, y);
        monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(id);
        NAME = monsterStrings.NAME;
        MOVES = monsterStrings.MOVES;
        DIALOG = monsterStrings.DIALOG;

        dialogX = -150.0f * Settings.scale;
        dialogY = 70.0f * Settings.scale;
        
        p = AbstractDungeon.player;
        moreDamageAs = ModHelper.moreDamageAscension(type);
        moreHPAs = ModHelper.moreHPAscension(type);
        specialAs = ModHelper.specialAscension(type);
    }
    
    public BaseMonster(String name, String id, int maxHealth, float hb_x, float hb_y, float hb_w, float hb_h, String imgUrl, float x, float y, boolean ignoreBlights) {
        super(name, id, maxHealth, hb_x, hb_y, hb_w, hb_h, imgUrl, x, y, ignoreBlights);
    }

    public BaseMonster(String id, String imgUrl, float hb_x, float hb_y, float hb_w, float hb_h, float x, float y) {
        this(CardCrawlGame.languagePack.getMonsterStrings(FGOMod.makeID(id)).NAME,
                id,
                1,
                hb_x, hb_y,
                hb_w, hb_h,
                imgUrl,
                x, y);
    }

    public BaseMonster(String id, float hb_x, float hb_y, float hb_w, float hb_h, float x, float y) {
        this(CardCrawlGame.languagePack.getMonsterStrings(FGOMod.makeID(id)).NAME,
                id,
                1,
                hb_x, hb_y,
                hb_w, hb_h,
                FGOMod.monsterPath(id),
                x, y);
    }

    public BaseMonster(String id, float hb_w, float hb_h, float x, float y) {
        this(CardCrawlGame.languagePack.getMonsterStrings(FGOMod.makeID(id)).NAME,
                id,
                1,
                0, -15F,
                hb_w, hb_h,
                FGOMod.monsterPath(id),
                x, y);
    }

    public BaseMonster(String id, float hb_w, float hb_h) {
        this(CardCrawlGame.languagePack.getMonsterStrings(FGOMod.makeID(id)).NAME,
                id,
                1,
                0, -15F,
                hb_w, hb_h,
                FGOMod.monsterPath(id),
                0, 0);
    }

    public BaseMonster process(Consumer<BaseMonster> consumer) {
        consumer.accept(this);
        return this;
    }

    public BaseMonster modifyHpByPercent(float percent) {
        setHp((int) (maxHealth * percent));
        return this;
    }

    public BaseMonster modifyHp(int modifyAmount) {
        setHp(maxHealth + modifyAmount);
        return this;
    }

    public BaseMonster setPreBattleAction(Consumer<BaseMonster> action) {
        preBattleAction = action;
        return this;
    }

    @Override
    public void usePreBattleAction() {
        super.usePreBattleAction();
        if (bgm != null) {
            AbstractDungeon.scene.fadeOutAmbiance();
            CardCrawlGame.music.silenceTempBgmInstantly();
            AbstractDungeon.getCurrRoom().playBgmInstantly(bgm);
        }
        if (preBattleAction != null) {
            preBattleAction.accept(this);
        }
    }

    @Override
    public void takeTurn() {
        if (nextMove >= 0 && nextMove < moveInfos.size()) {
            moveInfos.get(nextMove).move();
        } else {
            FGOMod.logger.info("Monster {} has no {} move!", NAME, nextMove);
        }
        addToBot(new RollMoveAction(this));
    }

    @Override
    public void update() {
        super.update();
        if (floatIndex != 0)
            animY = floatIndex * MathUtils.cosDeg((float) (System.currentTimeMillis() / 6L % 360L)) * 6.0f * Settings.scale;
    }

    public void addDamageActions(AbstractCreature target, int index, int numTimes, AbstractGameAction.AttackEffect effect) {
        for (int i = 0; i < numTimes; i++) {
            addToBot(new DamageAction(target, damage.get(index), effect));
        }
    }

    public void addMove(Intent intent, int dmg, Supplier<Integer> dmgTimeSupplier, Consumer<MoveInfo> takeMove) {
        int index = moveInfos.size();
        damage.add(new DamageInfo(this, dmg));
        moveInfos.add(new MoveInfo(index, intent, () -> damage.get(index).base, dmgTimeSupplier, takeMove));
    }

    public void addMoveA(Intent intent, int dmg, Supplier<Integer> dmgTime, Consumer<MoveInfo> takeMove) {
        addMove(intent, dmg, dmgTime, takeMove);
    }

    public void addMove(Intent intent, int dmg, int dmgTime, Consumer<MoveInfo> takeMove) {
        addMove(intent, dmg, () -> dmgTime, takeMove);
    }

    public void addMoveA(Intent intent, int dmg, int dmgTime, Consumer<MoveInfo> takeMove) {
        addMove(intent, dmg, () -> dmgTime, takeMove);
    }

    public void addMove(Intent intent, int dmg, Consumer<MoveInfo> takeMove) {
        addMove(intent, dmg, () -> 1, takeMove);
    }

    public void addMoveA(Intent intent, int dmg, Consumer<MoveInfo> takeMove) {
        addMove(intent, dmg, () -> 1, takeMove);
    }

    public void addMove(Intent intent, Consumer<MoveInfo> takeMove) {
        addMove(intent, 0, () -> 0, takeMove);
    }

    public void setMove(int i) {
        MoveInfo info = moveInfos.get(i);
        int dmgTime = info.damageTimeSupplier.get();
        if (dmgTime <= 0) {
            setMove(MOVES[i], (byte) i, info.intent);
        } else if (dmgTime == 1) {
            setMove(MOVES[i], (byte) i, info.intent, damage.get(i).base);
        } else {
            setMove(MOVES[i], (byte) i, info.intent, damage.get(i).base, dmgTime, true);
        }
    }

    public void attack(MoveInfo info, AbstractGameAction.AttackEffect effect, AttackAnim anim) {
        if (anim != null)
            switch (anim) {
                case FAST:
                    addToBot(new AnimateFastAttackAction(this));
                    break;
                case SLOW:
                    addToBot(new AnimateSlowAttackAction(this));
                    break;
                case HOP:
                    addToBot(new AnimateHopAction(this));
                    break;
                case JUMP:
                    addToBot(new AnimateJumpAction(this));
                    break;
                case MOVE:
                    addToBot(new VFXAction(new MoveToEffect(this, p.hb.cX + p.hb.width / 2 - hb.cX, p.hb.cY - hb.cY, true, 0.4f)));
                    break;
            }
        for (int i = 0; i < info.damageTimeSupplier.get(); i++) {
            addToBot(new DamageAction(p, damage.get(info.index), effect));
        }
    }

    public void attack(MoveInfo info, AbstractGameAction.AttackEffect effect) {
        attack(info, effect, null);
    }

    public String getLastMove() {
        return MOVES[MOVES.length - 1];
    }

    public void shout(int index, float volume) {
        if (index >= DIALOG.length) return;
        ModHelper.addToBotAbstract(() -> CardCrawlGame.sound.playV(getClass().getSimpleName() + "_" + index, volume));
        addToBot(new ShoutAction(this, DIALOG[index]));
    }

    public void shout(int index) {
        shout(index, 1.0f);
    }

    public void shout(int index, String sound) {
        shout(index, sound, 1.0f);
    }

    public void shout(int index, String sound, float volume) {
        if (index >= DIALOG.length) return;
        ModHelper.addToBotAbstract(() -> CardCrawlGame.sound.playV(sound, volume));
        addToBot(new ShoutAction(this, DIALOG[index]));
        
    }

    public void shout(int start, int end, float volume) {
        shout(AbstractDungeon.miscRng.random(start, end), volume);
    }

    public void shout(int start, int end) {
        shout(AbstractDungeon.miscRng.random(start, end), 3.0f);
    }

    public void shoutIf(int index) {
        if (AbstractDungeon.miscRng.randomBoolean()) {
            shout(index);
        }
    }

    public void shoutIf(int index, int chance) {
        if (chance < AbstractDungeon.miscRng.random(100)) {
            shout(index);
        }
    }
    
    public int getDamage(int normal, int ascension) {
        return ModHelper.moreDamageAscension(type) ? normal : ascension;
    }
    
    public static class MoveInfo {
        public int index;
        public AbstractMonster.Intent intent;
        public Consumer<MoveInfo> takeMove;
        public Supplier<Integer> damageSupplier;
        public Supplier<Integer> damageTimeSupplier;

        public MoveInfo(int index, Intent intent, Supplier<Integer> damageSupplier, Supplier<Integer> damageTimeSupplier, Consumer<MoveInfo> takeMove) {
            this.index = index;
            this.intent = intent;
            this.takeMove = takeMove;
            this.damageSupplier = damageSupplier;
            this.damageTimeSupplier = damageTimeSupplier;
        }

        public void move() {
            takeMove.accept(this);
        }
    }

    public enum AttackAnim {
        FAST,
        SLOW,
        HOP,
        JUMP,
        MOVE,
    }

    @SpirePatch(clz = AbstractMonster.class, method = SpirePatch.CONSTRUCTOR, paramtypez = {String.class, String.class, int.class, float.class, float.class, float.class, float.class, String.class, float.class, float.class, boolean.class})
    public static class MonsterMoveInfoPatch {
        @SpirePostfixPatch
        public static void Postfix(AbstractMonster __instance, String name, String id, int maxHealth, float hb_x, float hb_y, float hb_w, float hb_h, String imgUrl, float offsetX, float offsetY, boolean ignoreBlights, EnemyMoveInfo ___move) {
            if (__instance instanceof BaseMonster) {
                ((BaseMonster) __instance).currMove = ___move;
            }
        }
    }
}
