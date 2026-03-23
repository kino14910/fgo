package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateSlowAttackAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.combat.HemokinesisEffect;

import fgo.powers.CursePower;

public class Moss extends BaseMonster {
    public static final String ID = makeID(Moss.class.getSimpleName());
    public static final String IMG = monsterPath("Moss_Sprite");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] DIALOG = monsterStrings.DIALOG;
    
    private static final byte ATTACK_CURSE = 0;
    private static final byte STRENGTH_BUFF = 1;
    
    private static final int BASE_DMG = 6;
    private static final int BASE_RITUAL = 1;
    private static final int A2_RITUAL = 2;
    
    private final int ritualAmount;
    private boolean firstMove;

    public Moss() {
        this(0.0f, 0.0f);
    }

    public Moss(float x, float y) {
        super(NAME, ID, 54, 0.0f, 0.0f, 320.0f, 320.0f, IMG, x, y);
        if (AbstractDungeon.ascensionLevel >= 7) {
            setHp(50, 56);
        } else {
            setHp(48, 54);
        }
        ritualAmount = AbstractDungeon.ascensionLevel >= 2 ? A2_RITUAL : BASE_RITUAL;
        firstMove = true;
        
        final int dmg = BASE_DMG;
        
        addMoveA(Intent.ATTACK_DEBUFF, dmg, mi -> {
            addToBot(new AnimateSlowAttackAction(this));
            addToBot(new VFXAction(new HemokinesisEffect(hb.cX, hb.cY, p.hb.cX, p.hb.cY), 0.5f));
            attack(mi, AbstractGameAction.AttackEffect.NONE);
            addToBot(new ApplyPowerAction(p, this, new CursePower(p, this, 1), 1));
        });
        
        addMove(Intent.BUFF, mi -> {
            if (AbstractDungeon.ascensionLevel >= 17) {
                addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, ritualAmount + 1)));
            } else {
                addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, ritualAmount)));
            }
        });
    }

    @Override
    public void usePreBattleAction() {
    }

    @Override
    protected void getMove(int num) {
        if (firstMove) {
            firstMove = false;
            setMove(STRENGTH_BUFF);
        } else {
            setMove(ATTACK_CURSE);
        }
    }

    @Override
    public void die() {
        super.die();
    }
}

