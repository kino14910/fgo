package fgo.monsters;

import static fgo.FGOMod.makeID;
import static fgo.FGOMod.monsterPath;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.AnimateSlowAttackAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.GainBlockAction;
import com.megacrit.cardcrawl.actions.common.RollMoveAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.helpers.CardHelper;
import com.megacrit.cardcrawl.localization.MonsterStrings;
import com.megacrit.cardcrawl.powers.DexterityPower;
import com.megacrit.cardcrawl.powers.RitualPower;
import com.megacrit.cardcrawl.powers.StrengthPower;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import fgo.action.FgoNpAction;
import fgo.cards.noblecards.InnocenceAroundight;
import fgo.characters.Master;
import fgo.powers.ArtsCardPower;
import fgo.powers.BusterCardPower;
import fgo.powers.QuickCardPower;

public class FaerieKnightLancelot extends BaseMonster {
    public static final String ID = makeID(FaerieKnightLancelot.class.getSimpleName());
    public static final String IMG = monsterPath("FaerieKnightLancelot");
    private static final MonsterStrings monsterStrings = CardCrawlGame.languagePack.getMonsterStrings(ID);
    public static final String NAME = monsterStrings.NAME;
    public static final String[] MOVES = monsterStrings.MOVES;

    public FaerieKnightLancelot() {
        this(0.0f, 0.0f);
    }

    public FaerieKnightLancelot(float x, float y) {
        super(NAME, ID, 101, -20.0f, 0.0f, 240.0f, 320.0f, IMG, x, y);
        if (AbstractDungeon.ascensionLevel >= 8) {
            setHp(130, 134);
        } else {
            setHp(124, 128);
        }
        final int aroundightDmg = getDamage(14, 15);
        damage.add(new DamageInfo(this, aroundightDmg));
    }

    @Override
    public void usePreBattleAction() {
        addToBot(new ApplyPowerAction(this, this, new RitualPower(this, 3, false)));
        if (AbstractDungeon.ascensionLevel >= 18) {
            addToBot(new ApplyPowerAction(this, this, new StrengthPower(this, 3)));
        }
        AbstractPlayer p = AbstractDungeon.player;
        if (p instanceof Master) {
            // if (!CardHelper.hasCardWithID(Camelot.ID)) {
            //     AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new Camelot(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
            // }
            addToBot(new FgoNpAction(100));
            addToBot(new ApplyPowerAction(p, p, new BusterCardPower(p, 2)));
            addToBot(new ApplyPowerAction(p, p, new ArtsCardPower(p, 2)));
            addToBot(new ApplyPowerAction(p, p, new QuickCardPower(p, 10)));
        } else {
            addToBot(new GainBlockAction(p, p, 30));
            addToBot(new ApplyPowerAction(p, p, new StrengthPower(p, 3)));
            addToBot(new ApplyPowerAction(p, p, new DexterityPower(p, 3)));
        }
    }

    @Override
    public void takeTurn() {
        AbstractPlayer p = AbstractDungeon.player;
        if (nextMove == 1) {
            addToBot(new AnimateSlowAttackAction(this));
            addToBot(new DamageAction(p, damage.get(0), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        }
        addToBot(new RollMoveAction(this));
    }

    @Override
    protected void getMove(int num) {
        setMove(MOVES[0], (byte) 1, Intent.ATTACK, damage.get(0).base);
    }

    @Override
    public void die() {
        super.die();
        if (AbstractDungeon.player instanceof Master && !CardHelper.hasCardWithID(InnocenceAroundight.ID)) {
            AbstractDungeon.effectList.add(new ShowCardAndObtainEffect(new InnocenceAroundight(), Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f));
        }
    }
}
