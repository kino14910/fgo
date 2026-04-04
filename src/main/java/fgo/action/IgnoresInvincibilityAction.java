package fgo.action;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.red.Metallicize;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.powers.IntangiblePower;
import com.megacrit.cardcrawl.powers.PlatedArmorPower;

import fgo.powers.IgnoresInvincibilityPower;

public class IgnoresInvincibilityAction extends AbstractGameAction {
    private int timesAmount = 0;
    private final AbstractPlayer p = AbstractDungeon.player;
    private boolean usePower = false;

    public IgnoresInvincibilityAction(AbstractCreature target) {
        this(target, false);
    }

    public IgnoresInvincibilityAction(AbstractCreature target, boolean usePower) {
        this.target = target;
        actionType = ActionType.DAMAGE;
        duration = 0.1F;
        this.usePower = usePower;
    }

    @Override
    public void update() {
        if (duration == 0.1F && target != null) {
            if (target.currentBlock > 0) {
                target.currentBlock = 0;
                timesAmount++;
            }

            String[] powerIds = {
                IntangiblePower.POWER_ID,
                PlatedArmorPower.POWER_ID,
                Metallicize.ID
            };
            for (String id : powerIds) {
                if (target.hasPower(id)) {
                    addToBot(new RemoveSpecificPowerAction(target, p, id));
                    timesAmount++;
                }
            }

            if (usePower && timesAmount != 0) {
                addToBot(new ApplyPowerAction(p, p, new IgnoresInvincibilityPower(p, timesAmount)));
            }

            if (AbstractDungeon.getCurrRoom().monsters.areMonstersBasicallyDead()) {
                AbstractDungeon.actionManager.clearPostCombatActions();
            }
        }

        tickDuration();
    }
}
