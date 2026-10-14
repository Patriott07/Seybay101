using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menu Pause (ESC atau tombol): Resume, Restart, Setting, Home
public class PauseManager : MonoBehaviour
{
    // Cek ini dari script lain (stempel, drag dokumen, hover, dll) supaya tidak bisa dipakai saat pause
    public static bool IsPaused { get; private set; }

    [Header("Panel")]
    public GameObject panelPause;

    [Header("Tombol Pause Menu")]
    public Button tombolResume;
    public Button tombolRestart;
    public Button tombolSetting;
    public Button tombolHome;

    [Header("Tombol buka pause di HUD (opsional)")]
    public Button tombolBukaPause;

    [Header("Setting")]
    public SettingsPanel settingsPanel;

    [Header("Pengaturan")]
    public KeyCode tombolPause = KeyCode.Escape;
    [Tooltip("Nama scene Home (harus ada di Build Settings)")]
    public string namaSceneHome = "Home";
    [Tooltip("Jeda semua suara game saat pause")]
    public bool pauseAudioGame = false;
    [Tooltip("ESC menutup HP dulu kalau HP sedang terbuka")]
    public bool escTutupHpDulu = true;

    [Header("Restart")]
    [Tooltip("Aktifkan jika Restart harus memulai dari awal (save dihapus)")]
    public bool hapusSaveSaatRestart = false;
    public string namaFileSave = "game_save.json";

    void Start()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (panelPause != null) panelPause.SetActive(false);

        if (tombolResume != null) tombolResume.onClick.AddListener(Resume);
        if (tombolRestart != null) tombolRestart.onClick.AddListener(Restart);
        if (tombolSetting != null) tombolSetting.onClick.AddListener(BukaSetting);
        if (tombolHome != null) tombolHome.onClick.AddListener(KeHome);
        if (tombolBukaPause != null) tombolBukaPause.onClick.AddListener(Pause);

        if (settingsPanel != null)
            settingsPanel.onClosed.AddListener(() =>
            {
                if (IsPaused && panelPause != null) panelPause.SetActive(true);
            });
    }

    void OnDestroy()
    {
        // Jangan tinggalkan game dalam kondisi pause saat pindah scene
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(tombolPause)) TanganiTombolPause();
    }

    void TanganiTombolPause()
    {
        // 1. Kalau Setting terbuka: tutup, kembali ke menu pause
        if (settingsPanel != null && settingsPanel.IsOpen)
        {
            settingsPanel.Close();
            return;
        }

        // 2. Kalau HP terbuka: tutup HP dulu
        if (!IsPaused && escTutupHpDulu && PhoneManager.IsPhoneOpen && PhoneManager.Instance != null)
        {
            PhoneManager.Instance.TutupHp();
            return;
        }

        // 3. Toggle pause
        if (IsPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        if (panelPause != null) panelPause.SetActive(true);
        Time.timeScale = 0f;
        if (pauseAudioGame) AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        if (settingsPanel != null && settingsPanel.IsOpen) settingsPanel.Close();
        if (panelPause != null) panelPause.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void BukaSetting()
    {
        if (settingsPanel == null) return;
        if (panelPause != null) panelPause.SetActive(false);
        settingsPanel.Open();
    }

    public void Restart()
    {
        BersihkanSebelumPindah();

        if (hapusSaveSaatRestart)
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, namaFileSave);
                if (File.Exists(path)) File.Delete(path);
            }
            catch (IOException e)
            {
                Debug.LogWarning("Gagal menghapus save: " + e.Message);
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void KeHome()
    {
        BersihkanSebelumPindah();
        SceneManager.LoadScene(namaSceneHome);
    }

    void BersihkanSebelumPindah()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}