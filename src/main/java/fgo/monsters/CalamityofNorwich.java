package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateFastAttackAction;
import com.megacrit.cardcrawl.actions.animations.AnimateSlowAttackAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.FrailPower;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.powers.WeakPower;
import com.megacrit.cardcrawl.rooms.MonsterRoomBoss;
import com.megacrit.cardcrawl.vfx.BorderLongFlashEffect;
import com.megacrit.cardcrawl.vfx.combat.HemokinesisEffect;
import com.megacrit.cardcrawl.vfx.combat.ShockWaveEffect;
import com.megacrit.cardcrawl.vfx.combat.VerticalAuraEffect;

import fgo.powers.CursePower;
import fgo.powers.monster.CurseCondensingPower;
import fgo.powers.monster.PowerofGrudgePower;
import fgo.powers.monster.TaintedCursePower;

public class CalamityofNorwich extends BaseMonster {
    public static final String ID = makeID(CalamityofNorwich.class.getSimpleName());
    public static final String IMG = monsterPath("CalamityofNorwich_");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] MOVES = monsterStrings.MOVES;
    public static final String[] DIALOG = monsterStrings.DIALOG;
    
    private static final byte CURSE_ATTACK = 0;
    private static final byte DEBUFF_ATTACK = 1;
    private static final byte STEAL_CURSE = 2;
    private static final byte EXCRETION = 3;
    
    private static final int BASE_DMG = 7;
    private static final int A4_DMG = 8;
    private static final int BASE_HEAL = 30;
    private static final int A19_HEAL = 35;
    
    private int moveCount = 0;
    private final int healAmt;
    private boolean usedExcretion = false;
    
    public CalamityofNorwich() {
        this(0.0f, 0.0f);
    }
    
    public CalamityofNorwich(float x, float y) {
        super(NAME, ID, 140, 0.0f, 0.0f, 500.0f, 320.0f, IMG, x, y);
        type = EnemyType.BOSS;
        if (AbstractDungeon.ascensionLevel >= 9) {
            setHp(230);
        } else {
            setHp(220);
        }
        healAmt = AbstractDungeon.ascensionLevel >= 19 ? A19_HEAL : BASE_HEAL;
        
        final int dmg = getDamage(BASE_DMG, A4_DMG);
        
        addMoveA(Intent.ATTACK_DEBUFF, dmg, mi -> {
            addToBot(new VFXAction(this, new ShockWaveEffect(hb.cX, hb.cY, Settings.BLUE_TEXT_COLOR, ShockWaveEffect.ShockWaveType.CHAOTIC), 0.75f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.BLACK, hb.cX, hb.cY), 0.33f));
            addToBot(new SFXAction("ATTACK_FIRE"));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.PURPLE, hb.cX, hb.cY), 0.33f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.CYAN, hb.cX, hb.cY), 0.0f));
            addToBot(new VFXAction(this, new BorderLongFlashEffect(Color.MAGENTA), 0.0f, true));
            addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 2)));
            addToBot(new ApplyPowerAction(p, this, new CurseCondensingPower(p, 2)));
            addToBot(new AnimateFastAttackAction(this));
            addToBot(new VFXAction(new HemokinesisEffect(hb.cX, hb.cY, p.hb.cX, p.hb.cY), 0.5f));
            attack(mi, AbstractGameAction.AttackEffect.NONE);
        });
        
        addMoveA(Intent.ATTACK_DEBUFF, dmg, mi -> {
            addToBot(new VFXAction(this, new ShockWaveEffect(hb.cX, hb.cY, Settings.BLUE_TEXT_COLOR, ShockWaveEffect.ShockWaveType.CHAOTIC), 0.75f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.BLACK, hb.cX, hb.cY), 0.33f));
            addToBot(new SFXAction("ATTACK_FIRE"));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.PURPLE, hb.cX, hb.cY), 0.33f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.CYAN, hb.cX, hb.cY), 0.0f));
            addToBot(new VFXAction(this, new BorderLongFlashEffect(Color.MAGENTA), 0.0f, true));
            
            if (AbstractDungeon.ascensionLevel >= 19) {
                addToBot(new ApplyPowerAction(p, this, new WeakPower(p, 2, true)));
                addToBot(new ApplyPowerAction(p, this, new VulnerablePower(p, 2, true)));
                addToBot(new ApplyPowerAction(p, this, new FrailPower(p, 2, true)));
            } else {
                addToBot(new ApplyPowerAction(p, this, new WeakPower(p, 1, true), 1));
                addToBot(new ApplyPowerAction(p, this, new VulnerablePower(p, 1, true), 1));
                addToBot(new ApplyPowerAction(p, this, new FrailPower(p, 1, true), 1));
            }
            
            if (p.hasPower(CursePower.POWER_ID)) {
                int curAmt = p.getPower(CursePower.POWER_ID).amount;
                addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, curAmt), curAmt));
            }
            
            addToBot(new AnimateSlowAttackAction(this));
            addToBot(new VFXAction(new HemokinesisEffect(hb.cX, hb.cY, p.hb.cX, p.hb.cY), 0.5f));
            attack(mi, AbstractGameAction.AttackEffect.NONE);
        });
        
        addMoveA(Intent.ATTACK_DEFEND, dmg, mi -> {
            addToBot(new VFXAction(this, new ShockWaveEffect(hb.cX, hb.cY, Settings.BLUE_TEXT_COLOR, ShockWaveEffect.ShockWaveType.CHAOTIC), 0.75f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.BLACK, hb.cX, hb.cY), 0.33f));
            addToBot(new SFXAction("ATTACK_FIRE"));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.PURPLE, hb.cX, hb.cY), 0.33f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.CYAN, hb.cX, hb.cY), 0.0f));
            addToBot(new VFXAction(this, new BorderLongFlashEffect(Color.MAGENTA), 0.0f, true));
            
            if (p.hasPower(CursePower.POWER_ID)) {
                int curAmt = p.getPower(CursePower.POWER_ID).amount;
                addToBot(new RemoveSpecificPowerAction(p, this, CursePower.POWER_ID));
                addToBot(new ApplyPowerAction(this, this, new CursePower(this, this, curAmt), curAmt));
                addToBot(new GainBlockAction(this, this, 20));
                addToBot(new HealAction(this, this, 10));
            }
            
            addToBot(new AnimateSlowAttackAction(this));
            addToBot(new VFXAction(new HemokinesisEffect(hb.cX, hb.cY, p.hb.cX, p.hb.cY), 0.5f));
            attack(mi, AbstractGameAction.AttackEffect.NONE);
        });
        
        addMove(Intent.UNKNOWN, mi -> {
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.BLACK, hb.cX, hb.cY), 0.33f));
            addToBot(new SFXAction("ATTACK_FIRE"));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.PURPLE, hb.cX, hb.cY), 0.33f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.CYAN, hb.cX, hb.cY), 0.0f));
            addToBot(new VFXAction(this, new BorderLongFlashEffect(Color.MAGENTA), 0.0f, true));
            
            if (!hasPower(CursePower.POWER_ID)) return;
            int curAmt = getPower(CursePower.POWER_ID).amount;
            if (curAmt <= 14) {
                addToBot(new RemoveSpecificPowerAction(this, this, CursePower.POWER_ID));
                addToBot(new HealAction(this, this, healAmt));
                addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 3), 3));
            } else if (curAmt <= 29) {
                addToBot(new RemoveSpecificPowerAction(this, this, CursePower.POWER_ID));
                addToBot(new HealAction(this, this, healAmt * 2));
                addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 6), 6));
            } else {
                addToBot(new RemoveSpecificPowerAction(this, this, CursePower.POWER_ID));
                addToBot(new HealAction(this, this, healAmt * 3));
                addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 9), 9));
            }
        });
    }
    
    @Override
    public void usePreBattleAction() {
        super.usePreBattleAction();
        if (AbstractDungeon.getCurrRoom() instanceof MonsterRoomBoss) {
            CardCrawlGame.music.unsilenceBGM();
            AbstractDungeon.scene.fadeOutAmbiance();
            AbstractDungeon.getCurrRoom().playBgmInstantly("Grand_Battle3.mp3");
        }
        addToBot(new ApplyPowerAction(this, this, new TaintedCursePower(this)));
        addToBot(new ApplyPowerAction(this, this, new PowerofGrudgePower(this)));
    }
    
    @Override
    protected void getMove(int i) {
        if (currentHealth < maxHealth / 2 && !usedExcretion) {
            usedExcretion = true;
            setMove(EXCRETION);
        } else {
            switch (moveCount) {
                case 1: {
                    if (!lastTwoMoves(CURSE_ATTACK)) {
                        setMove(CURSE_ATTACK);
                    } else {
                        setMove(DEBUFF_ATTACK);
                    }
                    ++moveCount;
                    break;
                }
                case 2: {
                    if (!lastMove(DEBUFF_ATTACK)) {
                        setMove(DEBUFF_ATTACK);
                    } else {
                        setMove(STEAL_CURSE);
                    }
                    moveCount = 0;
                    break;
                }
                default: {
                    setMove(CURSE_ATTACK);
                    ++moveCount;
                }
            }
        }
    }
    
    @Override
    public void die() {
        super.die();
    }
}
