using UnityEngine;
using UnityEngine.Audio;

// Saved player settings and progress. Every PlayerPrefs key lives here.
// Save() on every write: WebGL only flushes PlayerPrefs to browser storage on Save(), closing the tab skips it.
public static class GameSettings
{
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        set { PlayerPrefs.SetFloat("MusicVolume", value); PlayerPrefs.Save(); }
    }
    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat("SfxVolume", 0.5f);
        set { PlayerPrefs.SetFloat("SfxVolume", value); PlayerPrefs.Save(); }
    }
    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        set { PlayerPrefs.SetFloat("Sensitivity", value); PlayerPrefs.Save(); }
    }
    public static int LevelUnlocked
    {
        get => PlayerPrefs.GetInt("LevelUnlocked", 0);
        set { PlayerPrefs.SetInt("LevelUnlocked", value); PlayerPrefs.Save(); }
    }
    public static bool ViewedTrainingVideo
    {
        get => PlayerPrefs.GetInt("ViewedTrainingVideo", 0) == 1;
        set { PlayerPrefs.SetInt("ViewedTrainingVideo", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    // Slider value (0-1) to mixer dB; 0 maps to silence instead of log10(0) = -infinity.
    public static float ToDecibels(float volume) => volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f;

    // Every key above lives in PlayerPrefs and nothing else is stored there, so clearing it all is a full reset.
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public static void ApplyVolumes(AudioMixer mixer)
    {
        mixer.SetFloat("Music", ToDecibels(MusicVolume));
        mixer.SetFloat("Sfx", ToDecibels(SfxVolume));
    }
}
