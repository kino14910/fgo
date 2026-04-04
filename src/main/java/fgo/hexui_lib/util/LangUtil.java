package fgo.hexui_lib.util;

import java.util.Random;

public class LangUtil {
    public static String pluralize(int amount, String text) {
        if (amount == 1) {
            return amount + " " + text;
        }
        return amount + " " + text + "s";
    }
    
    public static String pluralize(String text, int amount) {
        if (amount == 1) {
            return text;
        }
        return text + "s";
    }
    
    public static String pluralize(String singular, String plural, int amount) {
        if (amount == 1) {
            return singular;
        }
        return plural;
    }

    /*
    public static String getDescriptionFromActionList(ArrayList<DescriptiveAction> actionList) {
        String description = "";
        int totalGuessedLines = 0;
        for (DescriptiveAction action : actionList ){
            if (description != ""){
                description += " NL ";
                totalGuessedLines++;
            }
            description += action.getDescription();
            totalGuessedLines += action.getDescription().length()/25 + 1;
        }

        if (totalGuessedLines < 6){
            return description;
        }

        description = "";
        for (DescriptiveAction action : actionList ){
            if (description != ""){
                description += " NL ";
            }
            description += action.getShortDescription();
        }
        return description;
    }
    */

    public static long stringToSeed(String s) {
        if (s == null) {
            return 0;
        }
        long hash = 0;
        for (char c : s.toCharArray()) {
            hash = 31L * hash + c;
        }
        return hash;
    }

    public static Random getSeededRandom(long seed) {
        return new Random(seed);
    }

    public static Random getSeededRandom(String s) {
        return getSeededRandom(stringToSeed(s));
    }
}
