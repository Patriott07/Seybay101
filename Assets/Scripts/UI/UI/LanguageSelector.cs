using UnityEngine;
using UnityEngine.UI;

// Panel Language: pilih English / Indonesia
public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private Button englishButton;
    [SerializeField] private Button indonesianButton;

    private readonly Color selectedColor = new Color(0.35f, 0.7f, 1f);
    private readonly Color normalColor = Color.white;

    void OnEnable()
    {
        LanguageSetting.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        LanguageSetting.OnLanguageChanged -= Refresh;
    }

    public void SelectEnglish() { LanguageSetting.Set(GameLanguage.English); }
    public void SelectIndonesian() { LanguageSetting.Set(GameLanguage.Indonesian); }

    private void Refresh()
    {
        Tint(englishButton, LanguageSetting.Current == GameLanguage.English);
        Tint(indonesianButton, LanguageSetting.Current == GameLanguage.Indonesian);
    }

    private void Tint(Button b, bool selected)
    {
        if (b != null && b.targetGraphic != null)
            b.targetGraphic.color = selected ? selectedColor : normalColor;
    }
}
