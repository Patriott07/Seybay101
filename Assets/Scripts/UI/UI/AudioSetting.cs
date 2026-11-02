using UnityEngine;
using UnityEngine.UI;

// Panel Audio: slider BGM & SFX
public class AudioSetting : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    void OnEnable()
    {
        Setup(bgmSlider, GameAudio.BGM, GameAudio.SetBGM);
        Setup(sfxSlider, GameAudio.SFX, GameAudio.SetSFX);
    }

    void OnDisable()
    {
        if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(GameAudio.SetBGM);
        if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(GameAudio.SetSFX);
        PlayerPrefs.Save();
    }

    private void Setup(Slider slider, float current, UnityEngine.Events.UnityAction<float> onChanged)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.SetValueWithoutNotify(current);
        slider.onValueChanged.RemoveListener(onChanged);
        slider.onValueChanged.AddListener(onChanged);
    }
}
