package fgo.cards.colorless;

import java.util.ArrayList;

import com.megacrit.cardcrawl.actions.AbstractGameAction.AttackEffect;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.actions.common.DamageAction;
import com.megacrit.cardcrawl.cards.DamageInfo;
import com.megacrit.cardcrawl.cards.DamageInfo.DamageType;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.relics.AbstractRelic;
import com.megacrit.cardcrawl.relics.FrozenEye;

import fgo.cards.FGOCard;
import fgo.powers.ViyViyViyPower;

public class Shvibzik extends FGOCard {
    public static final String ID = makeID(Shvibzik.class.getSimpleName());
    
    public Shvibzik() {
        super(ID, 2, CardType.ATTACK, CardTarget.ENEMY, CardRarity.SPECIAL, CardColor.COLORLESS);
        setDamage(20, 5);
        setExhaust(true);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        addToBot(new DamageAction(m, new DamageInfo(p, damage, DamageType.NORMAL), AttackEffect.SLASH_HEAVY));
        
        // ViyViyViy functionality
        ArrayList<FrozenEye> rs = new ArrayList<>();
        if (!p.hasRelic("Frozen Eye")) {
            FrozenEye frozenEye = new FrozenEye();
            rs.add(frozenEye);
        }
        if (!rs.isEmpty()) {
            AbstractRelic abstractRelic = rs.get(0);
            AbstractDungeon.actionManager.addToBottom(new ApplyPowerAction(p, p, new ViyViyViyPower(p, abstractRelic)));
        }
    }
}
