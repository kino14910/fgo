package fgo.relics;

import static fgo.FGOMod.makeID;

import java.util.Collections;

import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.monsters.AbstractMonster;
import com.megacrit.cardcrawl.vfx.AbstractGameEffect;
import com.megacrit.cardcrawl.vfx.cardManip.ShowCardAndObtainEffect;

import fgo.cards.AbsNoblePhantasmCard;
import fgo.cards.FGOCard;
import fgo.cards.derivative.mash.BlackBarrel;
import fgo.cards.derivative.mash.Camelot;
import fgo.cards.derivative.mash.LordCamelot;
import fgo.cards.derivative.mash.LordChaldeas;
import fgo.cards.derivative.mash.ObscurantWallofChalk;
import fgo.cards.derivative.mash.VeneratedShieldOfSnowflakes;
import fgo.cards.derivative.mash.VeneratedWallOfSnowflakes;
import fgo.cards.derivative.mash.WallOfSnowflakes;
import fgo.characters.CustomEnums.FGOCardColor;
import fgo.ui.panels.NobleDeckCards;

public class LockChocolateStrawberry extends BaseRelic {
    private static final String NAME = LockChocolateStrawberry.class.getSimpleName();
	public static final String ID = makeID(NAME);
    private static BlackBarrel blackBarrel = new BlackBarrel();

    private static WallOfSnowflakes wall = new WallOfSnowflakes();
    private static VeneratedWallOfSnowflakes veneratedWall = new VeneratedWallOfSnowflakes();
    private static VeneratedShieldOfSnowflakes veneratedShield = new VeneratedShieldOfSnowflakes();

    private static ObscurantWallofChalk ObscurantWall = new ObscurantWallofChalk();

    private static Camelot camelot = new Camelot();
    private static LordCamelot lordCamelot = new LordCamelot();
    private static LordChaldeas lordChaldeas = new LordChaldeas();

    public LockChocolateStrawberry() {
        super(ID, NAME, FGOCardColor.FGO, RelicTier.SPECIAL, LandingSound.MAGICAL);
    }

    @Override
    public void onEquip() {
        Collections.addAll(AbstractDungeon.effectList,
            ObtainCardEffect(LockChocolateStrawberry.blackBarrel),
            ObtainCardEffect(LockChocolateStrawberry.wall),
            ObtainCardEffect(LockChocolateStrawberry.ObscurantWall)
        );
        NobleDeckCards.nobleCards.addCard(LockChocolateStrawberry.camelot);
    }

    private AbstractGameEffect ObtainCardEffect(FGOCard card) {
        return new ShowCardAndObtainEffect(card, Settings.WIDTH / 2.0f, Settings.HEIGHT / 2.0f);
    }

    @Override
    public String getUpdatedDescription() {
        return DESCRIPTIONS[0];
    }

    @Override
    public void onMonsterDeath(AbstractMonster m) {
        if (AbstractDungeon.actNum > 1 
                && m.type == AbstractMonster.EnemyType.BOSS 
                && AbstractDungeon.getCurrRoom().monsters.areMonstersBasicallyDead()) {
            changeNobleCard(LockChocolateStrawberry.lordCamelot, LockChocolateStrawberry.lordChaldeas);
            changeNobleCard(LockChocolateStrawberry.camelot, LockChocolateStrawberry.lordCamelot);
            changeCard(LockChocolateStrawberry.veneratedWall, LockChocolateStrawberry.veneratedShield);
            changeCard(LockChocolateStrawberry.wall, LockChocolateStrawberry.veneratedWall);
        }
    }

    private void changeCard(FGOCard oldCard, FGOCard newCard) {
        boolean hasCard = AbstractDungeon.player.masterDeck.group.stream()
            .anyMatch(card -> card.cardID.equals(oldCard.cardID));
        if (oldCard != null && hasCard) {
            if (oldCard.upgraded) {
                newCard.upgrade();
            }
            AbstractDungeon.player.masterDeck.group.removeIf(card -> card.cardID.equals(oldCard.cardID));
            AbstractDungeon.player.masterDeck.addToBottom(newCard);
        }
    }
    
    private void changeNobleCard(AbsNoblePhantasmCard oldCard, AbsNoblePhantasmCard newCard) {
        boolean hasCard = NobleDeckCards.nobleCards.group.stream()
            .anyMatch(card -> card.cardID.equals(oldCard.cardID));
        if (oldCard != null && hasCard) {
            if (oldCard.upgraded) {
                newCard.upgrade();
            }
            NobleDeckCards.nobleCards.group.removeIf(card -> card.cardID.equals(oldCard.cardID));
            NobleDeckCards.nobleCards.addCard(newCard);
        }
    }
}
