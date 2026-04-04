package fgo.cards.colorless;

import com.evacipated.cardcrawl.mod.stslib.actions.common.AllEnemyApplyPowerAction;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.animations.VFXAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInHandAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.actions.watcher.ChangeStanceAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.VulnerablePower;
import com.megacrit.cardcrawl.stances.WrathStance;
import com.megacrit.cardcrawl.vfx.combat.CleaveEffect;

import fgo.cards.FGOCard;

public class InfiniteSuffering extends FGOCard {
    public static final String ID = makeID(InfiniteSuffering.class.getSimpleName());

    public InfiniteSuffering() {
        super(ID, 2, CardType.ATTACK, CardTarget.ALL_ENEMY, CardRarity.SPECIAL, CardColor.COLORLESS);
        setDamage(8, 2);
        setMagic(2, 1);
        setExhaust();
        cardsToPreview = new TheAbsoluteSword();
    }

    @Override
    public void upgrade() {
        if (!upgraded) {
            upgradeName();
        }
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction("ATTACK_HEAVY"));
        addToBot(new VFXAction(p, new CleaveEffect(), 0.1F));
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.NONE));
        addToBot(new AllEnemyApplyPowerAction(p, magicNumber, 
            monster -> new VulnerablePower(monster, magicNumber, false))
        );
        addToBot(new ChangeStanceAction(WrathStance.STANCE_ID));
        addToBot(new MakeTempCardInHandAction(cardsToPreview.makeStatEquivalentCopy(), 1, true));
    }
}


