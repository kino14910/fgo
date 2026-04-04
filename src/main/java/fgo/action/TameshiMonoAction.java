package fgo.action;

import com.megacrit.cardcrawl.actions.AbstractGameAction;
import com.megacrit.cardcrawl.actions.common.ApplyPowerAction;
import com.megacrit.cardcrawl.characters.AbstractPlayer;
import com.megacrit.cardcrawl.core.CardCrawlGame;
import com.megacrit.cardcrawl.core.Settings;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.localization.UIStrings;
import com.megacrit.cardcrawl.screens.select.GridCardSelectScreen;

import basemod.ReflectionHacks;
import fgo.powers.StarPower;

public class TameshiMonoAction extends AbstractGameAction {

  private static final UIStrings uiStrings = CardCrawlGame.languagePack
      .getUIString("ExhaustAction");
  private static final String[] TEXT = uiStrings.TEXT;
  private AbstractPlayer p;
  private final int num;

  public TameshiMonoAction(int number) {
    actionType = AbstractGameAction.ActionType.CARD_MANIPULATION;
    p = AbstractDungeon.player;
    duration = Settings.ACTION_DUR_FAST;
    num = number;
  }

  @Override
  public void update() {
    GridCardSelectScreen screen = AbstractDungeon.gridSelectScreen;
    if (duration == Settings.ACTION_DUR_FAST) {
      if (p.discardPile.isEmpty()) {
        isDone = true;
        return;
      }
      screen.open(p.discardPile, num, true, TEXT[0]);
      tickDuration();
      return;
    }

    if (!screen.selectedCards.isEmpty()) {
      int selectedAmt = (int) ReflectionHacks.getPrivate(screen, GridCardSelectScreen.class, "cardSelectAmount");
      addToBot(new ApplyPowerAction(p, p, new StarPower(p, selectedAmt * 4)));
      screen.selectedCards.clear();
    }
    tickDuration();
  }
}