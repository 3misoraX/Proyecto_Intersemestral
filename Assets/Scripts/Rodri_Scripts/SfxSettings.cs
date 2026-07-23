using UnityEngine;

public static class SfxSettings
{
    private const string VolumeKey = "SfxVolume";

    public static float GetVolume()
    {
        return PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    public static void SetVolume(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value);
    }
}