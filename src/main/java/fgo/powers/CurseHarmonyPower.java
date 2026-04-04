package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.actions.unique.BouncingFlaskAction;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

public class CurseHarmonyPower extends BasePower {
    public static final String POWER_ID = makeID(CurseHarmonyPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public CurseHarmonyPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "ThirstForVengeancePower");
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        if (owner.hasPower(CursePower.POWER_ID) && !AbstractDungeon.getMonsters().areMonstersBasicallyDead()) {
            flash();
            int ChAmount = 0;
            ChAmount += (owner.getPower(CursePower.POWER_ID)).amount * amount;
            AbstractMonster randomMonster = AbstractDungeon.getMonsters().getRandomMonster(null, true, AbstractDungeon.cardRandomRng);
            addToBot(new BouncingFlaskAction(randomMonster, ChAmount, 1));
        }
    }
}
