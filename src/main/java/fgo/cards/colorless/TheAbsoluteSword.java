package fgo.cards.colorless;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.actions.watcher.ChangeStanceAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.DrawPower;
import com.megacrit.cardcrawl.vfx.combat.CleaveEffect;

import fgo.cards.FGOCard;

public class TheAbsoluteSword extends FGOCard {
    public static final String ID = makeID(TheAbsoluteSword.class.getSimpleName());

    public TheAbsoluteSword() {
        super(ID, 3, CardType.ATTACK, CardTarget.ALL_ENEMY, CardRarity.SPECIAL, CardColor.COLORLESS);
        setDamage(10, 3);
        setExhaust();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        for (int i = 0; i < 2; ++i) {
            addToBot(new SFXAction("ATTACK_HEAVY"));
            addToBot(new VFXAction(p, new CleaveEffect(), 0.1F));
            addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.NONE));
        }
        addToBot(new ApplyPowerAction(p, p, new DrawPower(p, 1)));
        addToBot(new ChangeStanceAction("Divinity"));
    }
}


