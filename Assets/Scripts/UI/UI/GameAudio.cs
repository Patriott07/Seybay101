using System;
using UnityEngine;

// Volume BGM & SFX (PlayerPrefs). Dipakai oleh AudioSetting (slider) dan AudioChannel (AudioSource).
public static class GameAudio
{
    private const string KeyBgm = "setting_bgm";
    private const string KeySfx = "setting_sfx";
    private static float bgm = -1f;
    private static float sfx = -1f;

    public static event Action OnVolumeChanged;

    public static float BGM
    {
        get { if (bgm < 0f) bgm = PlayerPrefs.GetFloat(KeyBgm, 1f); return bgm; }
    }

    public static float SFX
    {
        get { if (sfx < 0f) sfx = PlayerPrefs.GetFloat(KeySfx, 1f); return sfx; }
    }

    public static void SetBGM(float value)
    {
        bgm = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeyBgm, bgm);
        OnVolumeChanged?.Invoke();
    }

    public static void SetSFX(float value)
    {
        sfx = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(KeySfx, sfx);
        OnVolumeChanged?.Invoke();
    }
}
