package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateSlowAttackAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.FadingPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import fgo.cards.noblecards.BlackdogGalatine;
import fgo.characters.Master;
import fgo.powers.monster.FoulWeatherPower;

public class FaerieKnightGawain extends BaseMonster {
    public static final String ID = makeID(FaerieKnightGawain.class.getSimpleName());
    public static final String IMG = monsterPath("FaerieKnightGawain");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] DIALOG = monsterStrings.DIALOG;
    
    private static final byte BASH_ATTACK = 0;
    private static final byte HEAVY_ATTACK = 1;
    
    private static final int BASE_BASH_DMG = 6;
    private static final int A3_BASH_DMG = 8;
    private static final int BASE_HEAVY_DMG = 24;
    private static final int A3_HEAVY_DMG = 30;
    
    private int moveCount = 0;

    public FaerieKnightGawain() {
        this(0.0f, 0.0f);
    }

    public FaerieKnightGawain(float x, float y) {
        super(NAME, ID, 36, 0.0f, 0.0f, 320.0f, 320.0f, IMG, x, y);
        if (AbstractDungeon.ascensionLevel >= 8) {
            setHp(35, 40);
        } else {
            setHp(32, 36);
        }
        
        final int bashDmg = getDamage(BASE_BASH_DMG, A3_BASH_DMG);
        final int heavyDmg = getDamage(BASE_HEAVY_DMG, A3_HEAVY_DMG);
        
        addMoveA(Intent.ATTACK, bashDmg, mi -> {
            addToBot(new AnimateSlowAttackAction(this));
            attack(mi, AbstractGameAction.AttackEffect.BLUNT_HEAVY);
            if (AbstractDungeon.ascensionLevel >= 18) {
                addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 1)));
            }
        });
        
        addMoveA(Intent.ATTACK, heavyDmg, mi -> {
            addToBot(new AnimateSlowAttackAction(this));
            attack(mi, AbstractGameAction.AttackEffect.BLUNT_HEAVY);
        });
    }

    @Override
    public void usePreBattleAction() {
        addToBot(new ApplyPowerAction(this, this, new FoulWeatherPower(this, 70), 70));
        addToBot(new ApplyPowerAction(this, this, new FadingPower(this, 7)));
    }

    @Override
    protected void getMove(int num) {
        if (moveCount == 3) {
            setMove(HEAVY_ATTACK);
            moveCount = 0;
        } else {
            setMove(BASH_ATTACK);
            ++moveCount;
        }
    }

    @Override
    public void die() {
        super.die();
        if (AbstractDungeon.player instanceof Master && !CardHelper.hasCardWithID(BlackdogGalatine.ID)) {
            AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new BlackdogGalatine(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
        }
    }
}
