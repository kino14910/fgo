package fgo.cards.optionCards;

import static fgo.FGOMod.cardPath;

import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class RepairSpiritOrigin extends FGOCard {
    public static final String ID = makeID(RepairSpiritOrigin.class.getSimpleName());

    public RepairSpiritOrigin() {
        super(ID, -2, CardType.POWER, CardTarget.NONE, CardRarity.SPECIAL, CardColor.COLORLESS, cardPath("power/CommandSpellGuts"));
    }

    @Override
    public void upgrade() {
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) { onChoseThisOption(); }

    @Override
    public void onChoseThisOption() {
        addToBot(new HealAction(AbstractDungeon.player, AbstractDungeon.player, 30));
    }
}
