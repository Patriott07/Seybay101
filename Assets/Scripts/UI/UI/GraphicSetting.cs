using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Panel Graphic: pilih rasio layar + fullscreen
public class GraphicSetting : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown ratioDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    private const string KeyRatio = "setting_ratio";
    private const string KeyFull = "setting_fullscreen";

    private static readonly string[] Names = { "Native", "16:9", "16:10", "4:3", "21:9" };
    private static readonly Vector2[] Ratios =
    {
        Vector2.zero, new Vector2(16, 9), new Vector2(16, 10), new Vector2(4, 3), new Vector2(21, 9)
    };

    // Terapkan pilihan terakhir saat game start
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplySaved()
    {
        if (!PlayerPrefs.HasKey(KeyRatio) && !PlayerPrefs.HasKey(KeyFull)) return;
        int ratio = PlayerPrefs.GetInt(KeyRatio, 0);
        bool full = PlayerPrefs.GetInt(KeyFull, Screen.fullScreenMode != FullScreenMode.Windowed ? 1 : 0) == 1;
        Apply(ratio, full);
    }

    void OnEnable()
    {
        if (ratioDropdown != null)
        {
            ratioDropdown.ClearOptions();
            ratioDropdown.AddOptions(new List<string>(Names));
            ratioDropdown.SetValueWithoutNotify(Mathf.Clamp(PlayerPrefs.GetInt(KeyRatio, 0), 0, Names.Length - 1));
            ratioDropdown.RefreshShownValue();
            ratioDropdown.onValueChanged.RemoveListener(OnRatioChanged);
            ratioDropdown.onValueChanged.AddListener(OnRatioChanged);
        }

        if (fullscreenToggle != null)
        {
            bool full = PlayerPrefs.HasKey(KeyFull)
                ? PlayerPrefs.GetInt(KeyFull) == 1
                : Screen.fullScreenMode != FullScreenMode.Windowed;
            fullscreenToggle.SetIsOnWithoutNotify(full);
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }

    void OnDisable()
    {
        if (ratioDropdown != null) ratioDropdown.onValueChanged.RemoveListener(OnRatioChanged);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        PlayerPrefs.Save();
    }

    public void OnRatioChanged(int index)
    {
        PlayerPrefs.SetInt(KeyRatio, index);
        Apply(index, fullscreenToggle != null && fullscreenToggle.isOn);
    }

    public void OnFullscreenChanged(bool isOn)
    {
        PlayerPrefs.SetInt(KeyFull, isOn ? 1 : 0);
        Apply(ratioDropdown != null ? ratioDropdown.value : 0, isOn);
    }

    public static void Apply(int ratioIndex, bool fullscreen)
    {
        int nw = Display.main.systemWidth;
        int nh = Display.main.systemHeight;
        if (nw <= 0 || nh <= 0)
        {
            nw = Screen.currentResolution.width;
            nh = Screen.currentResolution.height;
        }

        // Resolusi terbesar dengan rasio pilihan yang muat di layar
        int w = nw, h = nh;
        if (ratioIndex > 0 && ratioIndex < Ratios.Length)
        {
            float target = Ratios[ratioIndex].x / Ratios[ratioIndex].y;
            float native = (float)nw / nh;
            if (target < native) w = Mathf.RoundToInt(nh * target);
            else h = Mathf.RoundToInt(nw / target);
        }

        if (fullscreen)
        {
            // Native -> borderless (aman), rasio lain -> exclusive
            FullScreenMode mode = ratioIndex == 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.ExclusiveFullScreen;
            Screen.SetResolution(w, h, mode);
        }
        else
        {
            // Mode jendela dibatasi 90% layar agar title bar/taskbar tidak keluar layar
            Screen.SetResolution(Mathf.RoundToInt(w * 0.9f), Mathf.RoundToInt(h * 0.9f), FullScreenMode.Windowed);
        }
    }
}
