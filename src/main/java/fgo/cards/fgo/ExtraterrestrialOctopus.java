package fgo.cards.fgo;

import static fgo.characters.CustomEnums.FGO_Foreigner;
import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;
import fgo.powers.StarPower;

public class ExtraterrestrialOctopus extends FGOCard {
    public static final String ID = makeID(ExtraterrestrialOctopus.class.getSimpleName());

    public ExtraterrestrialOctopus() {
        super(ID, 1, CardType.ATTACK, CardTarget.ENEMY, CardRarity.UNCOMMON);
        setDamage(0);
        setMagic(2, 1);
        tags.add(FGO_Foreigner);
    }

    @Override
    public void applyPowers() {
        setDamage(getPowerCount(AbstractDungeon.player, StarPower.POWER_ID));
        super.applyPowers();
        isDamageModified = isModified = true;
    }

    @Override
    public void onMoveToDiscard() {
        baseDamage = 0;
        super.applyPowers();
    }
    
    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage), AttackEffect.SLASH_DIAGONAL));
    }

    @Override
    public void triggerOnGlowCheck() {
        glowColor = AbstractDungeon.player.hasPower(StarPower.POWER_ID) 
            ? AbstractCard.GOLD_BORDER_GLOW_COLOR.cpy() 
            : AbstractCard.BLUE_BORDER_GLOW_COLOR.cpy();
    }
}

