package fgo.action;

import com.badlogic.gdx.graphics.Color;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.vfx.TextAboveCreatureEffect;

import fgo.characters.Master;
import fgo.powers.NPOverChargePower;

public class FgoNpAction extends AbstractGameAction {
    private final int amount;
    public boolean canText;
    
    public FgoNpAction(int amount) {
        this(amount, true);
    }

    public FgoNpAction(int amount, boolean canText) {
        this.amount = amount;
        this.canText = canText;
    }

    @Override
    public void update() {
        int oldNp = Master.fgoNp;
        // 0 <= fgoNP <=300
        Master.fgoNp = Math.min(Math.max(Master.fgoNp + amount, 0), 300);
        // 保留了一部分fgo特性，让你知道自己在玩fgo
        if (Master.fgoNp == 99 && oldNp < 99) {
            Master.fgoNp = 100;
        }

        AbstractPlayer p = AbstractDungeon.player;

        if (oldNp < 200 && Master.fgoNp >= 200) {
            addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(1)));
        }
        if (oldNp < 300 && Master.fgoNp == 300) {
            addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(1)));
        }

        if (oldNp >= 200 && Master.fgoNp < 200) {
            addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(-1)));
        }
        if (oldNp == 300 && Master.fgoNp < 300) {
            addToBot(new ApplyPowerAction(p, p, new NPOverChargePower(-1)));
        }

        if (canText) {
            String text = "NP" + (amount > 0 ? "+" : "") + amount + "%";
            AbstractDungeon.effectList.add(new TextAboveCreatureEffect(
            p.hb.cX - p.animX,
            p.hb.cY + p.hb.height / 2.0f,
            text,
            Color.WHITE.cpy()));
}

        if (p instanceof Master) {
            ((Master) p).TruthValueUpdatedEvent();
        }

        isDone = true;
    }
}
