package fgo.powers;

import static fgo.FGOMod.makeID;

import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;
import com.megacrit.cardcrawl.relics.AbstractRelic;

public class ViyViyViyPower extends BasePower {
    public static final String POWER_ID = makeID(ViyViyViyPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    private final AbstractRelic r;
    private final AbstractPlayer p;
    private final String rName;

    public ViyViyViyPower(AbstractCreature owner, AbstractRelic r) {
        super(POWER_ID, PowerType.BUFF, false, owner);
        this.r = r;
        this.p = AbstractDungeon.player;
        this.rName = r.name;
        AbstractDungeon.getCurrRoom().spawnRelicAndObtain(Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f, r);
        r.atBattleStart();
        updateDescription();
    }

    @Override
    public void onVictory() {
        this.p.loseRelic(this.r.relicId);
    }

    @Override
    public void updateDescription() {
        this.description = String.format(DESCRIPTIONS[0], this.rName);
    }
}
