package fgo.ui.panels;

import basemod.abstracts.CustomSavable;

public class MasterSkin implements CustomSavable<Integer> {
    public static int modifierIndexes = 0;

	@Override
    public void onLoad(Integer index) {
        if (index == null) {
            return;
        }
        modifierIndexes = index;
    }

	@Override
    public Integer onSave() {
        return modifierIndexes;
    }
}
