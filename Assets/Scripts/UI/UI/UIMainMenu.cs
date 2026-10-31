using Schema.data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject savePanel;              // Panel Save/Load
    [SerializeField] private GameObject settingPanel;           // berisi UISettings
    [SerializeField] private GameObject creditPanel;            // berisi UICredits
    [SerializeField] private GameObject quitConfirmationPanel;  // Yes / No

    [Header("Save Details (di dalam Save Panel)")]
    [SerializeField] private GameObject saveDetailContainer;
    [SerializeField] private TMP_Text saveDetailText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;

    private void Start()
    {
        Time.timeScale = 1f; // jaga-jaga kalau datang dari Pause
        ShowMainMenu();
    }

    // =========================================================
    // MAIN MENU
    // =========================================================
    public void ShowMainMenu()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (savePanel) savePanel.SetActive(false);
        if (settingPanel) settingPanel.SetActive(false);
        if (creditPanel) creditPanel.SetActive(false);
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(false);
    }

    // =========================================================
    // PLAY -> Save Panel
    // =========================================================
    public void OnClickPlay()
    {
        if (HasProgress(out GameSaveState data))
        {
            // Ada save: tampilkan detail + opsi Continue / New Game
            if (mainMenuPanel) mainMenuPanel.SetActive(false);
            if (savePanel) savePanel.SetActive(true);
            if (saveDetailContainer) saveDetailContainer.SetActive(true);
            if (continueButton) continueButton.gameObject.SetActive(true);
            if (newGameButton) newGameButton.gameObject.SetActive(true);
            ShowSaveDetail(data);
        }
        else
        {
            // Tidak ada save: otomatis New Game
            Debug.Log("[MainMenu] Tidak ada save. Membuat save baru otomatis.");
            OnClickNewGame();
        }
    }

    // SaveManager.Awake() otomatis membuat file save kosong jika belum ada,
    // sehingga HasSave() selalu true. Save yang masih default dianggap "tidak ada save".
    private bool HasProgress(out GameSaveState data)
    {
        data = null;
        SaveManager sm = SaveManager.Instance;
        if (sm == null || !sm.HasSave()) return false;

        data = sm.GetSaveData();
        if (data == null) return false;

        bool isFresh = data.currentDay <= 1
                       && data.playerCash <= 0
                       && data.purchasedTickets <= 0
                       && data.penalty <= 0
                       && data.trust >= 100;
        return !isFresh;
    }

    private void ShowSaveDetail(GameSaveState d)
    {
        if (saveDetailText == null || d == null) return;

        saveDetailText.text =
            $"<b>{LanguageSetting.T("Save Data Found", "Data Save Ditemukan")}</b>\n" +
            $"Day: {d.currentDay}\n" +
            $"Money: ${d.playerCash}\n" +
            $"Trust: {d.trust}% | Penalty: ${d.penalty}\n" +
            $"{LanguageSetting.T("Family Status", "Status Keluarga")}:\n" +
            FamilyLine("Aldo", d.purhaceTicketForAldo) + "\n" +
            FamilyLine("Nasya", d.purhaceTicketForNasya) + "\n" +
            FamilyLine("Virly", d.purhaceTicketForVirly) + "\n" +
            FamilyLine("Kraisa", d.purhaceTicketForKraisa);
    }

    private string FamilyLine(string name, bool safe)
    {
        string status = safe ? LanguageSetting.T("Safe", "Aman") : LanguageSetting.T("Not yet", "Belum");
        return $"  {name}: {status}";
    }

    // Tombol New Game
    public void OnClickNewGame()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.ResetSave();
        LoadGameScene();
    }

    // Tombol Continue
    public void OnClickContinue()
    {
        if (HasProgress(out _)) LoadGameScene();
        else OnClickNewGame();
    }

    // Tombol Back di Save Panel
    public void BackFromSave()
    {
        ShowMainMenu();
    }

    private void LoadGameScene()
    {
        if (!string.IsNullOrEmpty(gameSceneName)) SceneManager.LoadScene(gameSceneName);
        else Debug.LogError("[MainMenu] Game Scene belum diatur di Inspector!");
    }

    // =========================================================
    // SETTING (Back ditangani UISettings -> kembali ke Main Menu)
    // =========================================================
    public void OpenSetting()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (settingPanel) settingPanel.SetActive(true); // UISettings.OnEnable -> Language
    }

    // =========================================================
    // CREDIT (Back ditangani UICredits -> kembali ke Main Menu)
    // =========================================================
    public void OpenCredit()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (creditPanel) creditPanel.SetActive(true);
    }

    // =========================================================
    // QUIT (Yes / No)
    // =========================================================
    public void OpenQuitConfirmation()
    {
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(true);
    }

    public void QuitYes()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void QuitNo()
    {
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(false);
    }
}
