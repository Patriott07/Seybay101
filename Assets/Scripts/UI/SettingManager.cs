using UnityEngine;
using UnityEngine.Audio;

// Menyimpan & menerapkan volume. Taruh di objek ROOT (bukan anak objek lain) di scene Home.
// Objek ini tetap hidup saat pindah scene (DontDestroyOnLoad).
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    // Bisa dibaca script lain (mis. AudioManager) kalau tidak memakai AudioMixer
    public static float MasterVolume { get; private set; } = 1f;
    public static float BGMVolume { get; private set; } = 0.8f;
    public static float SFXVolume { get; private set; } = 0.8f;

    [Header("Audio Mixer (disarankan)")]
    public AudioMixer mixer;
    [Tooltip("Nama parameter yang di-Expose di Audio Mixer")]
    public string paramMaster = "MasterVolume";
    public string paramBGM = "BGMVolume";
    public string paramSFX = "SFXVolume";

    [Header("Nilai Awal (0 - 1)")]
    [Range(0f, 1f)] public float defaultMaster = 1f;
    [Range(0f, 1f)] public float defaultBGM = 0.8f;
    [Range(0f, 1f)] public float defaultSFX = 0.8f;

    const string KeyMaster = "setting_master";
    const string KeyBGM = "setting_bgm";
    const string KeySFX = "setting_sfx";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        MasterVolume = PlayerPrefs.GetFloat(KeyMaster, defaultMaster);
        BGMVolume = PlayerPrefs.GetFloat(KeyBGM, defaultBGM);
        SFXVolume = PlayerPrefs.GetFloat(KeySFX, defaultSFX);
        Terapkan();
    }

    void OnApplicationQuit() => Simpan();

    void OnDestroy()
    {
        if (Instance == this) Simpan();
    }

    public void SetMaster(float v) { MasterVolume = Mathf.Clamp01(v); Terapkan(); }
    public void SetBGM(float v) { BGMVolume = Mathf.Clamp01(v); Terapkan(); }
    public void SetSFX(float v) { SFXVolume = Mathf.Clamp01(v); Terapkan(); }

    public void Simpan()
    {
        PlayerPrefs.SetFloat(KeyMaster, MasterVolume);
        PlayerPrefs.SetFloat(KeyBGM, BGMVolume);
        PlayerPrefs.SetFloat(KeySFX, SFXVolume);
        PlayerPrefs.Save();
    }

    void Terapkan()
    {
        if (mixer != null)
        {
            mixer.SetFloat(paramMaster, KeDecibel(MasterVolume));
            mixer.SetFloat(paramBGM, KeDecibel(BGMVolume));
            mixer.SetFloat(paramSFX, KeDecibel(SFXVolume));
            AudioListener.volume = 1f;
        }
        else
        {
            // Tanpa mixer: hanya Master yang berfungsi otomatis.
            // BGM/SFX: kalikan volume AudioSource dengan SettingsManager.BGMVolume / SFXVolume.
            AudioListener.volume = MasterVolume;
        }
    }

    static float KeDecibel(float linear)
    {
        return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
    }
}