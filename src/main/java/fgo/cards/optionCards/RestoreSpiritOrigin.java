package fgo.cards.optionCards;

import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class RestoreSpiritOrigin extends FGOCard {
    public static final String ID = makeID(RestoreSpiritOrigin.class.getSimpleName());

    public RestoreSpiritOrigin() {
        super(ID, -2, CardType.POWER, CardTarget.NONE, CardRarity.SPECIAL, CardColor.COLORLESS);
        setNP(300);
    }
    
    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) { onChoseThisOption(); }

    @Override
    public void onChoseThisOption() {
        AbstractPlayer p = AbstractDungeon.player;
        addToBot(new HealAction(p, p, p.maxHealth));
        addToBot(new FgoNpAction(np));
    }
}
