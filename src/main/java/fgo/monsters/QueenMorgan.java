package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateFastAttackAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

// import fgo.Action.QueenMorganAction;
import fgo.cards.noblecards.RoadlessCamelot;
import fgo.characters.Master;
import fgo.powers.monster.FaeQueenPower;
import fgo.powers.monster.NowStormPower;
import fgo.powers.monster.OrkneyPower;
import fgo.powers.monster.ProphesizedChildPower;

public class QueenMorgan extends BaseMonster {
    public static final String ID = makeID(QueenMorgan.class.getSimpleName());
    public static final String IMG = monsterPath("QueenMorgan");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] MOVES = monsterStrings.MOVES;
    public static final String[] DIALOG = monsterStrings.DIALOG;
    
    private static final byte DOUBLE_FLAIL = 0;
    private static final byte FLAIL_BLOCK = 1;
    private static final byte BEAM_ATTACK = 2;
    private static final byte ORKNEY_BUFF = 3;
    private static final byte STORM_BUFF = 4;
    
    private static final int BASE_FLAIL_DMG = 12;
    private static final int A4_FLAIL_DMG = 14;
    private static final int BASE_BEAM_DMG = 18;
    private static final int A4_BEAM_DMG = 20;
    private static final int BASE_BLOCK = 10;
    private static final int A4_BLOCK = 12;
    
    private final int blockAmt;
    private int numTurns = 0;
    private int heavyAttack = 0;
    private boolean orkney;
    private boolean storm;

    public QueenMorgan() {
        this(0.0f, 0.0f);
    }

    public QueenMorgan(float x, float y) {
        super(NAME, ID, 300, 0.0f, 0.0f, 320.0f, 320.0f, IMG, x, y);
        type = EnemyType.BOSS;
        if (AbstractDungeon.ascensionLevel >= 9) {
            setHp(320);
        } else {
            setHp(300);
        }
        blockAmt = AbstractDungeon.ascensionLevel >= 4 ? A4_BLOCK : BASE_BLOCK;
        orkney = true;
        storm = true;
        
        final int flailDmg = getDamage(BASE_FLAIL_DMG, A4_FLAIL_DMG);
        final int beamDmg = getDamage(BASE_BEAM_DMG, A4_BEAM_DMG);
        
        addMoveA(Intent.ATTACK, flailDmg, 2, mi -> {
            addToBot(new AnimateFastAttackAction(this));
            attack(mi, AbstractGameAction.AttackEffect.SLASH_DIAGONAL);
            if (!hasPower(NowStormPower.POWER_ID)) {
            }
            // addToBot(new QueenMorganAction());
        });
        
        addMoveA(Intent.ATTACK_DEFEND, flailDmg, mi -> {
            addToBot(new AnimateFastAttackAction(this));
            attack(mi, AbstractGameAction.AttackEffect.SLASH_DIAGONAL);
            addToBot(new GainBlockAction(this, this, blockAmt));
            if (!hasPower(NowStormPower.POWER_ID)) {
            }
            // addToBot(new QueenMorganAction());
        });
        
        addMoveA(Intent.ATTACK, beamDmg, mi -> {
            addToBot(new AnimateFastAttackAction(this));
            attack(mi, AbstractGameAction.AttackEffect.BLUNT_HEAVY);
        });
        
        addMove(Intent.BUFF, mi -> {
            if (AbstractDungeon.ascensionLevel >= 19) {
                addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3), 3));
            } else {
                addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 2), 2));
            }
            addToBot(new ApplyPowerAction(this, this, new OrkneyPower(this, 5), 5));
        });
        
        addMove(Intent.BUFF, mi -> {
            addToBot(new ApplyPowerAction(this, this, new NowStormPower(this)));
        });
    }

    @Override
    public void usePreBattleAction() {
        super.usePreBattleAction();
        CardCrawlGame.music.unsilenceBGM();
        AbstractDungeon.scene.fadeOutAmbiance();
        AbstractDungeon.getCurrRoom().playBgmInstantly("BOSS_CITY");
        addToBot(new ApplyPowerAction(this, this, new FaeQueenPower(this)));
        addToBot(new ApplyPowerAction(p, p, new ProphesizedChildPower(p)));
    }

    @Override
    protected void getMove(int num) {
        if (numTurns == 3) {
            setMove(BEAM_ATTACK);
            numTurns = 0;
            ++heavyAttack;
        } else if (heavyAttack == 1 && orkney) {
            orkney = false;
            setMove(ORKNEY_BUFF);
        } else if (heavyAttack == 2 && storm) {
            storm = false;
            setMove(STORM_BUFF);
        } else {
            if (AbstractDungeon.aiRng.randomBoolean()) {
                setMove(DOUBLE_FLAIL);
            } else {
                setMove(FLAIL_BLOCK);
            }
            ++numTurns;
        }
    }

    @Override
    public void die() {
        super.die();
        if (!CardHelper.hasCardWithID(RoadlessCamelot.ID) && AbstractDungeon.player instanceof Master) {
            AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new RoadlessCamelot(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
        }
    }
}
