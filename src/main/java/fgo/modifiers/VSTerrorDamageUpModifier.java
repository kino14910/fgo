package fgo.modifiers;

import static fgo.FGOMod.makeID;

import com.evacipated.cardcrawl.mod.stslib.damagemods.AbstractDamageModifier;
import com.megacrit.cardcrawl.cards.AbstractCard;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.core.AbstractCreature;
import com.megacrit.cardcrawl.powers.AbstractPower;

import fgo.powers.TerrorPower;

public class VSTerrorDamageUpModifier extends AbstractDamageModifier {
    public static final String ID = makeID(VSTerrorDamageUpModifier.class.getSimpleName());
    private final int amount;
    
    public VSTerrorDamageUpModifier(int amount) {
        this.amount = amount;
    }
    
    @Override
    public float atDamageGive(float damage, DamageInfo.DamageType type, AbstractCreature target, AbstractCard card) {
        if (target == null) {
            return damage;
        }
        
        for (AbstractPower power : target.powers) {
            if (power instanceof TerrorPower) {
                float multiplier = 1.0f + amount / 100.0f;
                return damage * multiplier;
            }
        }
        
        return damage;
    }

    @Override
    public AbstractDamageModifier makeCopy() {
        return new VSTerrorDamageUpModifier(amount);
    }
}
