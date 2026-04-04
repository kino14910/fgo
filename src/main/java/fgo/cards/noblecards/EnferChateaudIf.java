package fgo.cards.noblecards;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.relics.AbstractRelic;
import com.megacrit.cardcrawl.relics.WingBoots;

import fgo.cards.AbsNoblePhantasmCard;

public class EnferChateaudIf extends AbsNoblePhantasmCard {
    public static final String ID = makeID(EnferChateaudIf.class.getSimpleName());

    public EnferChateaudIf() {
        super(ID, CardType.POWER, CardTarget.SELF, 1);
    }
    
    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (!p.hasRelic(WingBoots.ID)) {
            spawnRelicWingBoots();
            return;
        }

        AbstractRelic relic = p.getRelic(WingBoots.ID);
        if (relic != null && relic.counter == 0) {
            p.loseRelic(WingBoots.ID);
            spawnRelicWingBoots();
        }
    }

    private void spawnRelicWingBoots() {
        AbstractDungeon.getCurrRoom().spawnRelicAndObtain(Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f, new WingBoots());
    }
}
