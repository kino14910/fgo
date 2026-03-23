package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateFastAttackAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.actions.common.LoseHPAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInDiscardAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.PlatedArmorPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.BorderLongFlashEffect;
import com.megacrit.cardcrawl.vfx.combat.FlyingOrbEffect;
import com.megacrit.cardcrawl.vfx.combat.HemokinesisEffect;
import com.megacrit.cardcrawl.vfx.combat.VerticalAuraEffect;

import fgo.powers.CursePower;
import fgo.powers.InvincibilityTurnPower;
import fgo.powers.monster.CurseCondensingPower;
import fgo.powers.monster.CurseDecayedFleshPower;
import fgo.powers.monster.GainsCurseLayerPower;
import fgo.powers.monster.ProphesizedChildPower;

public class Cernunnos extends BaseMonster {
    public static final String ID = makeID(Cernunnos.class.getSimpleName());
    public static final String IMG = monsterPath("Cernunnos");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] DIALOG = monsterStrings.DIALOG;
    
    private static final byte CURSE_ATTACK = 0;
    private static final byte STRENGTH_BUFF = 1;
    private static final byte LIFE_DRAIN = 2;
    private static final byte CURSE_CONDENSE = 3;
    private static final byte ARMOR_HEAL = 4;
    private static final byte CURSE_DISASTER = 5;
    
    private static final int BASE_DMG = 10;
    private static final int A4_DMG = 12;
    private static final int BASE_STRENGTH = 2;
    private static final int A4_STRENGTH = 4;
    private static final int HP_AMT = 4;
    
    private final int strengthAmt;
    private boolean grimalkin;
    private boolean fae;
    private boolean condensing;
    
    public Cernunnos() {
        this(0.0f, 0.0f);
    }
    
    public Cernunnos(float x, float y) {
        super(NAME, ID, 500, 0.0f, 550.0f * Settings.scale, 320.0f, 320.0f, IMG, x, y - 550.0f * Settings.scale);
        type = EnemyType.BOSS;
        if (AbstractDungeon.ascensionLevel >= 9) {
            setHp(534);
        } else {
            setHp(500);
        }
        strengthAmt = AbstractDungeon.ascensionLevel >= 4 ? A4_STRENGTH : BASE_STRENGTH;
        grimalkin = true;
        fae = true;
        condensing = true;
        
        final int dmg = getDamage(BASE_DMG, A4_DMG);
        
        addMoveA(Intent.ATTACK_DEBUFF, dmg, mi -> {
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.BLACK, hb.cX, hb.cY), 0.33f));
            addToBot(new SFXAction("ATTACK_FIRE"));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.DARK_GRAY, hb.cX, hb.cY), 0.33f));
            addToBot(new VFXAction(this, new VerticalAuraEffect(Color.PURPLE, hb.cX, hb.cY), 0.0f));
            addToBot(new VFXAction(this, new BorderLongFlashEffect(Color.RED), 0.0f, true));
            addToBot(new AnimateFastAttackAction(this));
            addToBot(new VFXAction(new HemokinesisEffect(hb.cX, hb.cY, p.hb.cX, p.hb.cY), 0.5f));
            attack(mi, AbstractGameAction.AttackEffect.NONE);
            addToBot(new ApplyPowerAction(this, this, new CursePower(this, this, 1)));
        });
        
        addMove(Intent.BUFF, mi -> {
            addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, strengthAmt), strengthAmt));
            addToBot(new ApplyPowerAction(this, this, new InvincibilityTurnPower(this, 1)));
            if (p.currentHealth > HP_AMT) {
                for (int j = 0; j < HP_AMT; ++j) {
                    addToBot(new VFXAction(new FlyingOrbEffect(p.hb.cX, p.hb.cY)));
                }
                addToBot(new LoseHPAction(p, p, HP_AMT));
            } else {
                for (int j = 0; j < p.currentHealth; ++j) {
                    addToBot(new VFXAction(new FlyingOrbEffect(p.hb.cX, p.hb.cY)));
                }
                addToBot(new LoseHPAction(p, p, p.currentHealth));
            }
        });
        
        addMove(Intent.DEBUFF, mi -> {
            if (p.currentHealth > HP_AMT * 2) {
                for (int j = 0; j < HP_AMT * 2; ++j) {
                    addToBot(new VFXAction(new FlyingOrbEffect(p.hb.cX, p.hb.cY)));
                }
                addToBot(new LoseHPAction(p, p, HP_AMT * 2));
                addToBot(new HealAction(this, this, HP_AMT * 2));
            } else {
                for (int j = 0; j < p.currentHealth; ++j) {
                    addToBot(new VFXAction(new FlyingOrbEffect(p.hb.cX, p.hb.cY)));
                }
                addToBot(new LoseHPAction(p, p, p.currentHealth));
                addToBot(new HealAction(this, this, p.currentHealth));
            }
        });
        
        addMove(Intent.DEBUFF, mi -> {
            addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 1)));
            addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 1)));
            addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 2)));
            addToBot(new ApplyPowerAction(p, this, new CurseCondensingPower(p, 2)));
        });
        
        addMove(Intent.BUFF, mi -> {
            addToBot(new ReducePowerAction(this, this, CursePower.POWER_ID, 1));
            addToBot(new ApplyPowerAction(this, this, new PlatedArmorPower(this, 4)));
            addToBot(new HealAction(this, this, 10));
        });
        
        addMove(Intent.STRONG_DEBUFF, mi -> {
            addToBot(new ReducePowerAction(this, this, CursePower.POWER_ID, 3));
            if (AbstractDungeon.ascensionLevel >= 19) {
                addToBot(new MakeTempCardInDiscardAction(new fgo.cards.status.CurseDisaster(), 5));
            } else {
                addToBot(new MakeTempCardInDiscardAction(new fgo.cards.status.CurseDisaster(), 3));
            }
        });
    }
    
    @Override
    public void usePreBattleAction() {
        super.usePreBattleAction();
        CardCrawlGame.music.unsilenceBGM();
        AbstractDungeon.scene.fadeOutAmbiance();
        AbstractDungeon.getCurrRoom().playBgmInstantly("BOSS_BEYOND");
        addToBot(new ApplyPowerAction(this, this, new CurseDecayedFleshPower(this)));
        addToBot(new ApplyPowerAction(this, this, new GainsCurseLayerPower(this)));
        addToBot(new ApplyPowerAction(this, this, new CursePower(this, this, 7), 7));
        addToBot(new ApplyPowerAction(p, p, new ProphesizedChildPower(p)));
    }
    
    @Override
    protected void getMove(int num) {
        if (hasPower(CursePower.POWER_ID) && getPower(CursePower.POWER_ID).amount >= 11 && !lastMove(CURSE_DISASTER)) {
            setMove(CURSE_DISASTER);
        } else if (num < 70) {
            setMove(CURSE_ATTACK);
        } else if (grimalkin) {
            grimalkin = false;
            setMove(STRENGTH_BUFF);
        } else if (fae) {
            fae = false;
            setMove(LIFE_DRAIN);
        } else if (condensing) {
            condensing = false;
            setMove(CURSE_CONDENSE);
        } else {
            setMove(ARMOR_HEAL);
        }
    }
    
    @Override
    public void die() {
        super.die();
    }
}
