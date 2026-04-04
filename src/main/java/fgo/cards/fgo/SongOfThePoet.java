package fgo.cards.fgo;

import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.SongOfThePoetAction;
import fgo.cards.FGOCard;

public class SongOfThePoet extends FGOCard {
    public static final String ID = makeID(SongOfThePoet.class.getSimpleName());

    public SongOfThePoet() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(9, 3);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new SongOfThePoetAction(m, new DamageInfo(p, damage, damageTypeForTurn)));
    }
}


