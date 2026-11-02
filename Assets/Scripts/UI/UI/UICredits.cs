using UnityEngine;
using UnityEngine.SceneManagement;

public class UICredits : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private bool isSeparateScene = false;
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    [Header("Jika Credit berupa Panel")]
    [SerializeField] private GameObject mainMenuPanel;

    // Dipanggil oleh tombol Back
    public void BackToMainMenu()
    {
        if (isSeparateScene)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
