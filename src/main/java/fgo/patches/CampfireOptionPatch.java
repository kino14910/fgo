package fgo.patches;

import java.util.ArrayList;

import com.evacipated.cardcrawl.modthespire.lib.SpireInstrumentPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.megacrit.cardcrawl.dungeons.AbstractDungeon;
import com.megacrit.cardcrawl.rooms.CampfireUI;
import com.megacrit.cardcrawl.ui.campfire.AbstractCampfireOption;

import fgo.characters.Master;
import fgo.ui.campfire.AlterOption;
import javassist.CannotCompileException;
import javassist.expr.ExprEditor;
import javassist.expr.FieldAccess;

@SpirePatch(clz = CampfireUI.class, method = "initializeButtons")
public class CampfireOptionPatch {
    @SpireInstrumentPatch
    public static ExprEditor Instrument() {
        return new ExprEditor() {
            private boolean modified = false;

            @Override
            public void edit(FieldAccess f) throws CannotCompileException {
                if (!this.modified && f.getFieldName().equals("relics") && f.isReader()) {
                    this.modified = true;
                    f.replace(String.format("%s.insertCampfireOptions(this.buttons); $_ = $proceed($$);", CampfireOptionPatch.class.getName()));
                }
            }
        };
    }

    public static void insertCampfireOptions(ArrayList<AbstractCampfireOption> buttons) {
        if (AbstractDungeon.player instanceof Master)
            buttons.add(new AlterOption());
    }
}