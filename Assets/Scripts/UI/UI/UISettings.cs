using UnityEngine;

// Panel Setting: 3 sub panel (Language, Graphic, Audio). Panel pertama selalu Language.
public class UISettings : MonoBehaviour
{
    [Header("Sub Panels")]
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private GameObject graphicPanel;
    [SerializeField] private GameObject audioPanel;

    [Header("Back")]
    [Tooltip("Panel yang ditampilkan lagi saat Back ditekan (Main Menu / Pause Menu)")]
    [SerializeField] private GameObject returnPanel;

    void OnEnable()
    {
        ShowLanguage();
    }

    public void ShowLanguage() { Show(true, false, false); }
    public void ShowGraphic() { Show(false, true, false); }
    public void ShowAudio() { Show(false, false, true); }

    private void Show(bool language, bool graphic, bool audio)
    {
        if (languagePanel) languagePanel.SetActive(language);
        if (graphicPanel) graphicPanel.SetActive(graphic);
        if (audioPanel) audioPanel.SetActive(audio);
    }

    // Tombol Back -> kembali ke home (Main Menu) atau Pause Menu
    public void Back()
    {
        gameObject.SetActive(false);
        if (returnPanel) returnPanel.SetActive(true);
    }
}
