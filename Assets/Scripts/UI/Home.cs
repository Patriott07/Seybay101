using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Schema.data;

// Menu Home: New Game, Continue, Setting, Quit
// Terhubung dengan SaveManager (game_save.json / GameSaveState).
// Catatan: SaveManager hidup di scene Game, jadi di Home file save dibaca langsung.
public class HomeMenu : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Nama scene game (harus ada di Build Settings)")]
    public string namaSceneGame = "Game";

    [Header("Save File")]
    [Tooltip("Harus sama dengan nama file di SaveManager")]
    public string namaFileSave = "game_save.json";
    [Tooltip("Continue nonaktif jika save masih kosong (Day 1, uang 0, belum ada progres)")]
    public bool continueButuhProgres = true;

    [Header("Tombol")]
    public Button tombolNewGame;
    public Button tombolContinue;
    public Button tombolSetting;
    public Button tombolQuit;

    [Header("Info di dekat tombol Continue (opsional)")]
    [Tooltip("Contoh hasil: Hari 3  |  $120  |  Trust 80%")]
    public TextMeshProUGUI teksInfoContinue;

    [Header("Setting")]
    public SettingsPanel settingsPanel;

    [Header("Konfirmasi New Game (opsional)")]
    public GameObject panelKonfirmasi;
    public Button tombolYa;
    public Button tombolTidak;

    [Header("Event")]
    [Tooltip("Dipanggil saat New Game, sebelum scene dimuat")]
    public UnityEvent saatNewGame;

    private bool bisaContinue;

    string PathSave => Path.Combine(Application.persistentDataPath, namaFileSave);

    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (tombolNewGame != null) tombolNewGame.onClick.AddListener(NewGame);
        if (tombolContinue != null) tombolContinue.onClick.AddListener(Continue);
        if (tombolSetting != null) tombolSetting.onClick.AddListener(BukaSetting);
        if (tombolQuit != null) tombolQuit.onClick.AddListener(Quit);

        if (tombolYa != null) tombolYa.onClick.AddListener(MulaiBaru);
        if (tombolTidak != null) tombolTidak.onClick.AddListener(() => panelKonfirmasi.SetActive(false));

        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(false);

        SegarkanContinue();
    }

    // ==========================================
    // --- SAVE ---
    // ==========================================

    void SegarkanContinue()
    {
        GameSaveState s = null;
        bisaContinue = TryBacaSave(out s) && SaveValid(s) && (!continueButuhProgres || !SaveMasihBaru(s));

        if (tombolContinue != null) tombolContinue.interactable = bisaContinue;

        if (teksInfoContinue != null)
            teksInfoContinue.text = bisaContinue
                ? "Hari " + s.currentDay + "  |  $" + s.playerCash + "  |  Trust " + s.trust + "%"
                : "";
    }

    bool TryBacaSave(out GameSaveState hasil)
    {
        hasil = null;
        try
        {
            if (!File.Exists(PathSave)) return false;
            hasil = JsonUtility.FromJson<GameSaveState>(File.ReadAllText(PathSave));
            return hasil != null;
        }
        catch (IOException e)
        {
            Debug.LogWarning("Gagal membaca save: " + e.Message);
            return false;
        }
    }

    // Aturan sama dengan SaveManager.IsSaveValid()
    static bool SaveValid(GameSaveState s)
    {
        if (s == null) return false;
        if (s.playerCash < 0) return false;
        if (s.currentDay < 1) return false;
        if (s.trust < 0 || s.trust > 100) return false;
        return true;
    }

    // Save bawaan CreateNewSave() = belum ada progres
    static bool SaveMasihBaru(GameSaveState s)
    {
        return s.currentDay <= 1
            && s.playerCash <= 0
            && s.purchasedTickets <= 0
            && s.penalty <= 0
            && s.trust >= 100;
    }

    // ==========================================
    // --- TOMBOL ---
    // ==========================================

    public void NewGame()
    {
        // Tanya dulu hanya jika ada progres yang akan hilang
        if (bisaContinue && panelKonfirmasi != null)
            panelKonfirmasi.SetActive(true);
        else
            MulaiBaru();
    }

    void MulaiBaru()
    {
        saatNewGame?.Invoke();

        // Sama seperti SaveManager.ResetSave(): hapus file.
        // Scene Game akan membuat save baru otomatis (LoadSave -> CreateNewSave).
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSave();
        }
        else
        {
            try
            {
                if (File.Exists(PathSave)) File.Delete(PathSave);
            }
            catch (IOException e)
            {
                Debug.LogWarning("Gagal menghapus save: " + e.Message);
            }
        }

        SceneManager.LoadScene(namaSceneGame);
    }

    public void Continue()
    {
        if (!bisaContinue) return;
        // SaveManager di scene Game memuat game_save.json sendiri (Awake -> LoadSave)
        SceneManager.LoadScene(namaSceneGame);
    }

    public void BukaSetting()
    {
        if (settingsPanel != null) settingsPanel.Open();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}