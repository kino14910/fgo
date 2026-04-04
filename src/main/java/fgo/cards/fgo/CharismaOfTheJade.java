package fgo.cards.fgo;

import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.cards.DamageInfo.DamageType;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.StarPower;

public class CharismaOfTheJade extends FGOCard {
    public static final String ID = makeID(CharismaOfTheJade.class.getSimpleName());

    public CharismaOfTheJade() {
        super(ID, 2, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(7);
        setMagic(3, 1);
    }

    /**
     * {@link StarPower#atDamageGive(float, DamageType, AbstractCard)}
     */
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        if (getPowerCount(p, StarPower.POWER_ID) >= 20) {
            addToBot(new ApplyPowerAction(p, p, new StarPower(p, -10)));
        }

        for (int i = 0; i < magicNumber; i++) {
            addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        }
    }

    @Override
    public void triggerOnGlowCheck() {
        if (!AbstractDungeon.player.hasPower(StarPower.POWER_ID))
            return;
        glowColor = AbstractDungeon.player.getPower(StarPower.POWER_ID).amount < 20 
                    ? AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy()
                    : AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy();
    }
}

