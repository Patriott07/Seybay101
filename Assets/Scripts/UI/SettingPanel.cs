using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Panel Setting: BGM, SFX, Master Volume.
// Pasang di objek yang SELALU AKTIF (mis. objek kosong "SettingsRoot"),
// lalu isi 'panel' dengan panel setting yang boleh nonaktif.
public class SettingsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject panel;
    public Button tombolKembali;

    [Header("Slider (0 - 1)")]
    public Slider sliderMaster;
    public Slider sliderBGM;
    public Slider sliderSFX;

    [Header("Teks persen (opsional)")]
    public TextMeshProUGUI teksMaster;
    public TextMeshProUGUI teksBGM;
    public TextMeshProUGUI teksSFX;

    [Header("Event")]
    public UnityEvent onClosed;

    public bool IsOpen => panel != null && panel.activeSelf;

    void Start()
    {
        SiapkanSlider(sliderMaster, v => { SettingsManager.Instance?.SetMaster(v); UpdateTeks(); });
        SiapkanSlider(sliderBGM, v => { SettingsManager.Instance?.SetBGM(v); UpdateTeks(); });
        SiapkanSlider(sliderSFX, v => { SettingsManager.Instance?.SetSFX(v); UpdateTeks(); });

        if (tombolKembali != null) tombolKembali.onClick.AddListener(Close);

        if (panel != null) panel.SetActive(false);
    }

    void SiapkanSlider(Slider s, UnityAction<float> aksi)
    {
        if (s == null) return;
        s.minValue = 0f;
        s.maxValue = 1f;
        s.wholeNumbers = false;
        s.onValueChanged.AddListener(aksi);
    }

    public void Open()
    {
        if (panel == null) return;
        panel.SetActive(true);

        // Samakan slider dengan nilai tersimpan tanpa memicu event
        sliderMaster?.SetValueWithoutNotify(SettingsManager.MasterVolume);
        sliderBGM?.SetValueWithoutNotify(SettingsManager.BGMVolume);
        sliderSFX?.SetValueWithoutNotify(SettingsManager.SFXVolume);
        UpdateTeks();
    }

    public void Close()
    {
        if (panel == null || !panel.activeSelf) return;

        SettingsManager.Instance?.Simpan();
        panel.SetActive(false);
        onClosed?.Invoke();
    }

    void UpdateTeks()
    {
        if (teksMaster != null) teksMaster.text = Persen(SettingsManager.MasterVolume);
        if (teksBGM != null) teksBGM.text = Persen(SettingsManager.BGMVolume);
        if (teksSFX != null) teksSFX.text = Persen(SettingsManager.SFXVolume);
    }

    static string Persen(float v) => Mathf.RoundToInt(v * 100f) + "%";
}