package fgo.cards.fgo;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.actions.common.GainEnergyAction;
import com.megacrit.cardcrawl.actions.common.MakeTempCardInHandAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.cards.tempCards.SoulOfWaterChannels;
import fgo.powers.WatersidePower;

public class JourneyGuidance extends FGOCard {
    public static final String ID = makeID(JourneyGuidance.class.getSimpleName());

    public JourneyGuidance() {
        super(ID, 2, CardType.ATTACK, CardTarget.ENEMY, CardRarity.COMMON);
        setDamage(14, 4);
        cardsToPreview = new SoulOfWaterChannels();
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, damageTypeForTurn), AbstractGameAction.AttackEffect.BLUNT_HEAVY));
        if (p.hasPower(WatersidePower.POWER_ID)) {
            addToBot(new GainEnergyAction(2));
        }
        addToBot(new MakeTempCardInHandAction(cardsToPreview.makeStatEquivalentCopy(), 1));
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = AbstractDungeon.player.hasPower(WatersidePower.POWER_ID)
                ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy()
                : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }
}


