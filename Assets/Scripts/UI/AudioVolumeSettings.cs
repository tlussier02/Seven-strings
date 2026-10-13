using UnityEngine;

// Shared preference keys let dynamically created music sources load the same setting.
public static class AudioVolumeSettings
{
    public const string MasterVolumeKey = "SevenStrings.MasterVolume";
    public const string MusicVolumeKey = "SevenStrings.MusicVolume";

    public static float MasterVolume => Load(MasterVolumeKey);
    public static float MusicVolume => Load(MusicVolumeKey);

    private static float Load(string key)
    {
        float value = PlayerPrefs.GetFloat(key, 1f);
        return float.IsNaN(value) || float.IsInfinity(value) ? 1f : Mathf.Clamp01(value);
    }
}
