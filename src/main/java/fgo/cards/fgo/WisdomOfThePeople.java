package fgo.cards.fgo;

import java.util.ArrayList;

import com.megacrit.cardcrawl.actions.common.HealAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.action.FgoNpAction;
import fgo.cards.FGOCard;

public class WisdomOfThePeople extends FGOCard {
    public static final String ID = makeID(WisdomOfThePeople.class.getSimpleName());

    public WisdomOfThePeople() {
        super(ID, 3, CardType.SKILL, CardTarget.SELF, CardRarity.RARE);
        setMagic(20);
        setNP(30);
        setExhaust();
        tags.add(CardTags.HEALING);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new HealAction(p, p, magicNumber));
        if (upgraded) {
            addToBot(new FgoNpAction(np));
        }

        if (p.powers.isEmpty()) {
            return;
        }
        ArrayList<AbstractPower> pows = new ArrayList<>();
        for (AbstractPower pow : p.powers) {
            if (pow.type == AbstractPower.PowerType.DEBUFF) {
                pows.add(pow);
            }
        }
        if (!pows.isEmpty()) {
            AbstractPower po = pows.get(AbstractDungeon.miscRng.random(0, pows.size() - 1));
            addToBot(new RemoveSpecificPowerAction(p, p, po));
        }
    }
}


