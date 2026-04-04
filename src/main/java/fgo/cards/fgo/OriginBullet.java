package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.utility.SFXAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.IgnoresInvincibilityAction;
import fgo.cards.FGOCard;
import fgo.utils.Sounds;

public class OriginBullet extends FGOCard {
    public static final String ID = makeID(OriginBullet.class.getSimpleName());

    public OriginBullet() {
        super(ID, 1, CardType.SKILL, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setExhaust(false, true);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SFXAction(Sounds.gun));
        addToBot(new IgnoresInvincibilityAction(m, true));
    }
}


