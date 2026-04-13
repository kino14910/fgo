package fgo.cards.fgo;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.StrengthPower;

import fgo.cards.FGOCard;
import fgo.characters.CustomEnums.FGOCardColor;

public class GodsExecution extends FGOCard {
    public static final String ID = makeID(GodsExecution.class.getSimpleName());
    private boolean playedNoblePhantasm = false;

    public GodsExecution() {
        super(ID, 3, CardType.ATTACK, CardTarget.ALL_ENEMY, CardRarity.UNCOMMON);
        setDamage(21, 7);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageType, AttackEffect.NONE, true));
        addToBot(new AllEnemyApplyPowerAction(p, -2, 
            monster -> new StrengthPower(monster, -2)
        ));
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = this.playedNoblePhantasm
            ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy()
            : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }

    @Override
    public void onPlayCard(AbstractCard c, AbstractMonster m) {
        if (c.color == FGOCardColor.NOBLE_PHANTASM) {
            this.playedNoblePhantasm = true;
            this.freeToPlayOnce = true;
        }
    }
}
