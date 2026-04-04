package fgo.cards.fgo;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.helpers.ImageMaster;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.ItsInevitablePower;
import fgo.utils.Sounds;

public class ItsInevitable extends FGOCard {
    public static final String ID = makeID(ItsInevitable.class.getSimpleName());

    public ItsInevitable() {
        super(ID, 1, CardType.ATTACK, CardTarget.ALL_ENEMY, CardRarity.COMMON);
        setDamage(4, 1);
        setMagic(4, 1);
        portraitImg = ImageMaster.loadImage(cardPath("attack/ItsInevitable"));
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction(Sounds.gun));
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.FIRE));
        addToBot(new ApplyPowerAction(p, p, new ItsInevitablePower(p, damage, magicNumber)));
    }
}


