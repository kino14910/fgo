package fgo.ui.panels;

import static fgo.FGOMod.makeID;

import basemod.EasyConfigPanel;
import fgo.FGOMod;

public class FGOConfig extends EasyConfigPanel {
    public static int baseNPPerCost = 3;
    
    public static boolean enableColorlessCards = true;
    public static boolean enablePadoru = true;
    public static boolean enableFtue = true;
    public static boolean enableNoCostNoblePhantasm = true;
    
    public static boolean enableEmiya = true;
    public static boolean enableCalamityofNorwich = true;
    public static boolean enableCernunnos = true;
    public static boolean enableFaerieKnightGawain = true;
    public static boolean enableFaerieKnightLancelot = true;
    public static boolean enableMoss = true;
    public static boolean enableQueenMorgan = true;
    

    public FGOConfig() {
        super(FGOMod.modID, makeID(FGOConfig.class.getSimpleName()));
        
        setNumberRange("baseNPPerCost", 1, 5);
    }

}