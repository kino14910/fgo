package fgo.patches;

import java.util.HashSet;
import java.util.Set;

import com.badlogic.gdx.audio.Music;
import com.evacipated.cardcrawl.modthespire.lib.SpirePatch;
import com.evacipated.cardcrawl.modthespire.lib.SpirePostfixPatch;
import com.evacipated.cardcrawl.modthespire.lib.SpireReturn;
import com.megacrit.cardcrawl.audio.MainMusic;
import com.megacrit.cardcrawl.audio.TempMusic;

import basemod.BaseMod;

@SpirePatch(
        clz = TempMusic.class,
        method = "getSong")
public class TempMusicPatch {
    // Set to store all audio file names from the music directory
    private static final Set<String> MUSIC_FILES = new HashSet<>();
    
    // Static initializer to populate the set with music file names
    static {
        MUSIC_FILES.add("Grand_Battle3.mp3");
        MUSIC_FILES.add("Shimousa.mp3");
        MUSIC_FILES.add("UBW_Extended.mp3");
    }
    
    //Lets you start custom music from e.g. an elite fight.
    @SpirePostfixPatch
    public static SpireReturn<Music> Prefix(TempMusic __instance, String key) {
        if(MUSIC_FILES.contains(key)) {
            BaseMod.logger.info("Starting custom music: {}", key);
            return SpireReturn.Return(MainMusic.newMusic("fgo/audio/music/" + key));
        }
        return SpireReturn.Continue();
    }

}