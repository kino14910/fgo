package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.action.FgoNpAction;

public class WorldsEndFlowerGardenPower extends BasePower {
    public static final String POWER_ID = makeID(WorldsEndFlowerGardenPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public WorldsEndFlowerGardenPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount, "FightToDeathPower");
    }

    @Override
    public void onAttack(DamageInfo info, int damageAmount, AbstractCreature target) {
        if (damageAmount > 0 
                && target != owner 
                && info.type == DamageInfo.DamageType.NORMAL 
                && owner.hasPower(StarPower.POWER_ID)
                && owner.getPower(StarPower.POWER_ID).amount >= 10) {
            flash();
            addToBot(new FgoNpAction(amount, false));
        }
    }
}
