using TMPro;
using UnityEngine;

// Tempel di TMP_Text mana pun agar teksnya ikut berganti EN/ID.
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    public string english;
    [TextArea] public string indonesian;

    private TMP_Text label;

    void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        LanguageSetting.OnLanguageChanged += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        LanguageSetting.OnLanguageChanged -= Refresh;
    }

    private void Refresh()
    {
        if (label != null) label.text = LanguageSetting.T(english, indonesian);
    }
}
