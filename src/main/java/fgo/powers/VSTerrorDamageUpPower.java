package fgo.powers;

import static fgo.FGOMod.makeID;

import java.util.Collections;
import java.util.List;

import com.evacipated.cardcrawl.mod.stslib.damagemods.AbstractDamageModifier;
import com.evacipated.cardcrawl.mod.stslib.powers.interfaces.DamageModApplyingPower;
import com.megacrit.cardcrawl.actions.common.RemoveSpecificPowerAction;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.localization.PowerStrings;

import fgo.modifiers.VSTerrorDamageUpModifier;

public class VSTerrorDamageUpPower extends BasePower implements DamageModApplyingPower {
    public static final String POWER_ID = makeID(VSTerrorDamageUpPower.class.getSimpleName());
    public static final PowerStrings powerStrings = CardCrawlGame.languagePack.getPowerStrings(POWER_ID);
    
    public VSTerrorDamageUpPower(AbstractCreature owner, int amount) {
        super(POWER_ID, PowerType.BUFF, true, owner, amount, "AttackUpPower");
    }

    @Override
    public void atEndOfTurn(boolean isPlayer) {
        if (isPlayer) addToBot(new RemoveSpecificPowerAction(owner, owner, POWER_ID));
    }

    @Override
    public boolean shouldPushMods(DamageInfo info, Object source, List<AbstractDamageModifier> mods) {
        return source instanceof AbstractCard && ((AbstractCard) source).type == AbstractCard.CardType.ATTACK && mods.stream().noneMatch(mod -> mod instanceof VSTerrorDamageUpModifier);
    }

    @Override
    public List<AbstractDamageModifier> modsToPush(DamageInfo info, Object source, List<AbstractDamageModifier> mods) {
        return Collections.singletonList(new VSTerrorDamageUpModifier(amount));
    }
}
