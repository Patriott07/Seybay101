using UnityEngine;

// Tempel di AudioSource musik (BGM) atau efek suara (SFX) agar volumenya mengikuti slider Audio.
[RequireComponent(typeof(AudioSource))]
public class AudioChannel : MonoBehaviour
{
    public enum Channel { BGM, SFX }
    public Channel channel = Channel.SFX;

    private AudioSource source;
    private float baseVolume = 1f;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        baseVolume = source.volume; // volume asli dari Inspector dijadikan acuan
    }

    void OnEnable()
    {
        GameAudio.OnVolumeChanged += Apply;
        Apply();
    }

    void OnDisable()
    {
        GameAudio.OnVolumeChanged -= Apply;
    }

    private void Apply()
    {
        if (source == null) return;
        source.volume = baseVolume * (channel == Channel.BGM ? GameAudio.BGM : GameAudio.SFX);
    }
}
