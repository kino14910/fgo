package fgo.action;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.vfx.combat.ThrowDaggerEffect;

public class SongOfThePoetAction extends AbstractGameAction {
    private final DamageInfo info;
    
    public SongOfThePoetAction(AbstractCreature target, DamageInfo info) {
        this.info = info;
        setValues(target, info);
        actionType = ActionType.DAMAGE;
        startDuration = Settings.ACTION_DUR_FAST;
        duration = startDuration;
    }

    @Override
    public void update() {
        if (shouldCancelAction()) {
            isDone = true;
            return;
        }

        tickDuration();
        if (!isDone) {
            return;
        }

        target.damage(info);
        if (target.lastDamageTaken > 0) {
            addToTop(new FgoNpAction(target.lastDamageTaken));
        }
        if (target != null && target.hb != null) {
            addToTop(new VFXAction(new ThrowDaggerEffect(target.hb.cX, target.hb.cY)));
        }
    }
}
