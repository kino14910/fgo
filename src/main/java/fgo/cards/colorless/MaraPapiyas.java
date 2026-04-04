package fgo.cards.colorless;

import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class MaraPapiyas extends FGOCard {
    public static final String ID = makeID(MaraPapiyas.class.getSimpleName());

    public MaraPapiyas() {
        super(ID, 0, CardType.SKILL, CardTarget.SELF, CardRarity.UNCOMMON, CardColor.COLORLESS);
        setMagic(12, 4);
        tags.add(CardTags.HEALING);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        AbstractDungeon.player.decreaseMaxHealth(2);
        addToBot(new HealAction(p, p, magicNumber));
    }
}


