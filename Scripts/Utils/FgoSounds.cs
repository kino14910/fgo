using STS2RitsuLib.Audio;

namespace Fgo.Scripts.Utils;

public record FgoSounds
{
    public static readonly StreamingResourceMusicSource UBW_Music = new(MusicPath("UBW_Extended.mp3"));
    public static readonly ResourceSoundFileSource MasterChoose = new(SoundPath("MASTER_CHOOSE.mp3"));
    public static readonly ResourceSoundFileSource Padoru = new(SoundPath("Padoru.mp3"));
    public static readonly ResourceSoundFileSource MasterCurse = new(SoundPath("MASTER_CURSE.wav"));

    public static readonly ResourceSoundFileSource MasterInvictusSpiritus =
        new(SoundPath("MASTER_INVICTUS_SPIRITUS.mp3"));

    public static readonly ResourceSoundFileSource Gun = new(SoundPath("hermit_gun2.ogg"));
    public static readonly ResourceSoundFileSource Kanshou = new(SoundPath("S011_Attack6.ogg"));
    public static readonly ResourceSoundFileSource Sokoda = new(SoundPath("S011_Attack4.ogg"));
    public static readonly ResourceSoundFileSource Segawaruku = new(SoundPath("S011_Skill1.ogg"));
    public static readonly ResourceSoundFileSource TraceOn = new(SoundPath("S011_Skill2.ogg"));
    public static readonly ResourceSoundFileSource Konosaida = new(SoundPath("S011_Skill3.ogg"));

    public static readonly ResourceSoundFileSource UBW_Incantation = new(SoundPath("UBW.ogg"));

    private static string MusicPath(string file)
    {
        return $"res://Fgo/audio/music/{file}";
    }

    private static string SoundPath(string file)
    {
        return $"res://Fgo/audio/sound/{file}";
    }
}