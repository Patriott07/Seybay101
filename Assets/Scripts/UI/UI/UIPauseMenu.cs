using UnityEngine;
using UnityEngine.SceneManagement;

// Pause: ESC atau klik tombol -> Resume, Setting, Main Menu, Quit (Yes/No)
public class UIPauseMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("UI")]
    [SerializeField] private GameObject pauseRoot;              // overlay gelap (induk semua panel pause)
    [SerializeField] private GameObject menuPanel;              // Resume / Setting / Main Menu / Quit
    [SerializeField] private UISettings settings;
    [SerializeField] private GameObject quitConfirmationPanel;
    [SerializeField] private GameObject pauseButton;            // tombol pause di HUD

    public bool IsPaused { get; private set; }

    private void Start()
    {
        CloseAll();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (!EscapePressed()) return;

        if (quitConfirmationPanel != null && quitConfirmationPanel.activeSelf) QuitNo();
        else if (settings != null && settings.gameObject.activeSelf) settings.Back();
        else if (IsPaused) Resume();
        else Pause();
    }

    private static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.InputSystem.Keyboard.current != null
               && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    // Tombol pause di HUD / ESC
    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pauseRoot) pauseRoot.SetActive(true);
        if (menuPanel) menuPanel.SetActive(true);
        if (settings) settings.gameObject.SetActive(false);
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(false);
        if (pauseButton) pauseButton.SetActive(false);
    }

    public void Resume()
    {
        CloseAll();
    }

    private void CloseAll()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pauseRoot) pauseRoot.SetActive(false);
        if (menuPanel) menuPanel.SetActive(false);
        if (settings) settings.gameObject.SetActive(false);
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(false);
        if (pauseButton) pauseButton.SetActive(true);
    }

    // Back dari Setting kembali ke menuPanel (diatur UISettings.returnPanel)
    public void OpenSetting()
    {
        if (menuPanel) menuPanel.SetActive(false);
        if (settings) settings.gameObject.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OpenQuitConfirmation()
    {
        if (quitConfirmationPanel) quitConfirmationPanel.SetActive(true);
    }

    public void QuitYes()
    {
        Time.timeScale = 1f;
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
