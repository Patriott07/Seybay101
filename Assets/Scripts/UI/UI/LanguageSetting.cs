using System;
using UnityEngine;

public enum GameLanguage { English = 0, Indonesian = 1 }

// Penyimpan bahasa (PlayerPrefs). Bukan bagian dari save system game.
public static class LanguageSetting
{
    private const string Key = "setting_language";
    private static bool loaded;
    private static GameLanguage current;

    public static event Action OnLanguageChanged;

    public static GameLanguage Current
    {
        get
        {
            if (!loaded)
            {
                current = (GameLanguage)PlayerPrefs.GetInt(Key, 0);
                loaded = true;
            }
            return current;
        }
    }

    public static void Set(GameLanguage language)
    {
        current = language;
        loaded = true;
        PlayerPrefs.SetInt(Key, (int)language);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke();
    }

    // Helper: LanguageSetting.T("Play", "Main")
    public static string T(string en, string id)
    {
        return Current == GameLanguage.Indonesian ? id : en;
    }
}
