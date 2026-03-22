package fgo.ui.panels;

import static fgo.FGOMod.makeID;

import basemod.EasyConfigPanel;
import fgo.FGOMod;

public class FGOConfig extends EasyConfigPanel {
    public static boolean enableColorlessCards = true;
    public static boolean enableEmiya = true;
    public static boolean enablePadoru = true;
    public static boolean enableFtue = true;
    public static boolean enableNoCostNoblePhantasm = true;
    

    public FGOConfig() {
        super(FGOMod.modID, makeID(FGOConfig.class.getSimpleName()));
    }

}