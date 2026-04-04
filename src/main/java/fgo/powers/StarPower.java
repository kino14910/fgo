package fgo.powers;

import static fgo.FGOMod.makeID;
import static fgo.characters.CustomEnums.FGO_Foreigner;
import static fgo.utils.ModHelper.getPowerCount;

import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.ReducePowerAction;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.actions.utility.UseCardAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.cards.fgo.CharismaOfTheJade;
import fgo.characters.CustomEnums.FGOCardColor;

public class StarPower extends BasePower {
    public static final String POWER_ID = makeID(StarPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);

    public StarPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, false, owner, amount);
    }

    @Override
    public void updateDescription() {
        description = amount < 10 ? DESCRIPTIONS[0] : DESCRIPTIONS[1];
    }

    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type, AbstractCard card) {
        //如果你有20颗暴击星，使用翡翠的魅力时，暴击威力增加200%。
        //如果你有20颗暴击星且拥有星月夜能力，使用领域外生命的卡时，暴击威力增加200%。
        if (CharismaOfTheJade.ID.equals(card.cardID) && amount >= 20 
                || AbstractDungeon.player.hasPower(ForeignerPower.POWER_ID) && card.hasTag(FGO_Foreigner) && amount >= 10) {
            return finalDamage(damage, type, 3.0f);
        }

        //你有10颗暴击星时才能暴击。
        if (card.color != FGOCardColor.NOBLE_PHANTASM && amount >= 10) {
            return finalDamage(damage, type, 2.0f);
        }

        return damage;
    }
    
    public float finalDamage(float damage, DamageInfo.DamageType type, float multiplier) {
        int starHunterAmt = 0;
        if (owner.hasPower(StarHunterPower.POWER_ID)) {
            BasePower starHunterPower = (BasePower) owner.getPower(StarHunterPower.POWER_ID);
            starHunterAmt = starHunterPower.amount2;
        }

        int crAmt = getPowerCount(owner, CriticalDamageUpPower.POWER_ID) + starHunterAmt;
        return type == DamageInfo.DamageType.NORMAL
                ? damage * (multiplier + crAmt / 100.0f)
                : damage;
    }

    @Override
    public void onUseCard(AbstractCard card, UseCardAction action) {
        //在你打出攻击牌时，且有10颗以上暴击星，且不是宝具牌。
        if (card.type != AbstractCard.CardType.ATTACK || amount < 10 || card.color == FGOCardColor.NOBLE_PHANTASM)
            return;
        
        //在有十二辉剑时，有卡牌暴击时获得一层十二辉剑效果。
        if (owner.hasPower(HeroicKingPower.POWER_ID)) {
            addToBot(new ApplyPowerAction(owner, owner, new HeroicKingPower(owner, 1)));
        }
        if (owner.hasPower(LoseCritDamagePower.POWER_ID)) {
            addToBot(new RemoveSpecificPowerAction(owner, owner, LoseCritDamagePower.POWER_ID));
        }

        addToBot(new ReducePowerAction(owner, owner, ID, card.cardID.equals(CharismaOfTheJade.ID) && amount >= 20 ? 20 : 10));

    }
}
