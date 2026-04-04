package fgo.ui.panels;

import static fgo.FGOMod.uiPath;

import com.badlogic.gdx.graphics.Texture;
import com.megacrit.cardcrawl.helpers.ImageMaster;

import basemod.abstracts.CustomSavable;

public class CommandSpellPanel implements CustomSavable<Integer> {
    public static int commandSpellCount = 3;
    public static Texture CommandSpell;

    @Override
    public Integer onSave() {
        return commandSpellCount;
    }

    @Override
    public void onLoad(Integer count) {
         if (count == null) {
            return;
        }
        commandSpellCount = count;
    }

    public static void reset() {
        commandSpellCount = 3;
        CommandSpell = ImageMaster.loadImage(uiPath("CommandSpell/CommandSpell3"));
    }
}
