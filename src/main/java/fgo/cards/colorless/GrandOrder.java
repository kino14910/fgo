package fgo.cards.colorless;

import com.evacipated.cardcrawl.mod.stslib.fields.cards.AbstractCard.FleetingField;
import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.DamageAllEnemiesAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;

import fgo.cards.FGOCard;

public class GrandOrder extends FGOCard {
    public static final String ID = makeID(GrandOrder.class.getSimpleName());

    public GrandOrder() {
        super(ID, 1, CardType.ATTACK, CardTarget.ALL_ENEMY, CardRarity.RARE, CardColor.COLORLESS);
        setDamage(9999);
        setCostUpgrade(0);
        FleetingField.fleeting.set(this, true);
    }

    @Override
    public void use(AbstractPlayer p, AbstractMonster m) {
        m = AbstractDungeon.getMonsters().monsters.stream()
                .filter(monster -> monster.type != AbstractMonster.EnemyType.BOSS)
                .findFirst()
                .orElse(null);
        if (m == null) {
            return;
        }
        addToBot(new DamageAllEnemiesAction(p, multiDamage, damageTypeForTurn, AbstractGameAction.AttackEffect.SLASH_HEAVY));
    }
}


