using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource _musicSource;

    [SerializeField]
    private AudioSource _sfxSource;

    [SerializeField]
    private AudioSource _ambientSource;

    [Header("Music Clips")]
    [SerializeField]
    private AudioClip _ambientAuto;

    [SerializeField]
    private AudioClip _bgmAuto;

    [Header("SFX Clips")]
    [SerializeField]
    private AudioClip _sfxClickButton;

    [SerializeField]
    private AudioClip _sfxCoin;

    [SerializeField]
    private AudioClip _sfxFootstep;

    [SerializeField]
    private AudioClip _sfxPaper;

    [Header("Settings")]
    [SerializeField]
    private float _musicVolume = 0.5f;

    [SerializeField]
    private float _sfxVolume = 0.8f;

    [SerializeField]
    private float _ambientVolume = 0.4f;

    [SerializeField]
    private float _fadeDuration = 1f;

    [SerializeField]
    private bool autoPlayingAmbient = true;

    [SerializeField]
    private bool autoPlayingMusic = false;

    private bool _isMusicPlaying;
    private bool _isAmbientPlaying;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _musicSource.volume = _musicVolume;
        _sfxSource.volume = _sfxVolume;
        _ambientSource.volume = _ambientVolume;
    }

    void Start()
    {
        if(autoPlayingMusic)
            PlayBgm(_bgmAuto);
        if (autoPlayingAmbient)
            PlayAmbient(_ambientAuto);
    }

    #region Music

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null)
            return;
        // Hentikan coroutine fade yang sedang berjalan agar tidak bentrok
        StopAllCoroutines();
        StartCoroutine(ChangeBgmRoutine(clip));
    }

    private IEnumerator ChangeBgmRoutine(AudioClip newClip)
    {
        // 1. Jika ada musik yang sedang berputar, lakukan Fade Out terlebih dahulu
        if (_musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutMusic());
        }

        // 2. Lakukan Fade In untuk musik baru
        yield return StartCoroutine(FadeInMusic(newClip));
    }

    public void PlayBgmLoop(AudioClip clip)
    {
        if (clip == null)
            return;
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
        _isMusicPlaying = true;
    }

    public void StopBgm()
    {
        StartCoroutine(FadeOutMusic());
        _isMusicPlaying = false;
    }

    public void ToggleMusic(bool enabled)
    {
        if (enabled && !_isMusicPlaying && _musicSource.clip != null)
        {
            _musicSource.Play();
            _isMusicPlaying = true;
        }
        else if (!enabled && _isMusicPlaying)
        {
            _musicSource.Stop();
            _isMusicPlaying = false;
        }
    }

    public void PlayMysteryBell()
    {
        if (_bgmAuto != null)
            _sfxSource.PlayOneShot(_bgmAuto);
    }

    #endregion

    #region Ambience

    public void PlayAmbient(AudioClip clip)
    {
        if (clip == null)
            return;
        _ambientSource.clip = clip;
        _ambientSource.loop = true;
        _ambientSource.Play();
        _isAmbientPlaying = true;
    }

    public void StopAmbient()
    {
        if (_ambientSource != null && _ambientSource.isPlaying)
        {
            StartCoroutine(FadeOutAmbient());
            _isAmbientPlaying = false;
        }
    }

    public void ToggleAmbient(bool enabled)
    {
        if (enabled && !_isAmbientPlaying && _ambientSource.clip != null)
        {
            _ambientSource.Play();
            _isAmbientPlaying = true;
        }
        else if (!enabled && _isAmbientPlaying)
        {
            _ambientSource.Stop();
            _isAmbientPlaying = false;
        }
    }

    #endregion

    #region SFX

    public void PlaySfxClick()
    {
        if (_sfxClickButton != null)
            _sfxSource.PlayOneShot(_sfxClickButton);
    }

    public void PlaySfxCoin()
    {
        if (_sfxCoin != null)
            _sfxSource.PlayOneShot(_sfxCoin);
    }

    public void PlaySfxFootstep()
    {
        if (_sfxFootstep != null)
            _sfxSource.PlayOneShot(_sfxFootstep);
    }

    public void PlaySfxPaper()
    {
        if (_sfxPaper != null)
            _sfxSource.PlayOneShot(_sfxPaper);
    }

    public void PlaySfxApprove()
    {
        PlaySfxClick();
        PlaySfxCoin();
    }

    public void PlaySfxReject()
    {
        PlaySfxClick();
        if (_sfxPaper != null)
            _sfxSource.PlayOneShot(_sfxPaper);
    }

    public void PlaySfxSceneTransition()
    {
        PlaySfxClick();
        PlaySfxPaper();
    }

    public void PlaySfxNpcSpawn()
    {
        PlaySfxFootstep();
        // PlaySfxPaper();
    }

    public void PlaySfxLuggageOpen()
    {
        PlaySfxPaper();
        PlaySfxCoin();
    }

    public void PlaySfxLuggageClose()
    {
        PlaySfxPaper();
    }

    public void PlaySfxViolation()
    {
        if (_sfxClickButton != null)
            _sfxSource.PlayOneShot(_sfxClickButton);
        if (_sfxPaper != null)
            _sfxSource.PlayOneShot(_sfxPaper);
    }

    public void PlaySfxTimerEnd()
    {
        PlaySfxCoin();
        PlaySfxPaper();
    }

    public void PlaySfxLose()
    {
        _sfxSource.pitch = 0.5f;
        PlaySfxClick();
        _sfxSource.pitch = 1f;
    }

    public void PlaySfxMenuToggle()
    {
        PlaySfxClick();
    }

    public void PlaySfxTicket()
    {
        PlaySfxPaper();
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        _musicSource.volume = _musicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        _sfxSource.volume = _sfxVolume;
    }

    public void SetAmbientVolume(float volume)
    {
        _ambientVolume = Mathf.Clamp01(volume);
        _ambientSource.volume = _ambientVolume;
    }

    public float GetMusicVolume() => _musicVolume;

    public float GetSfxVolume() => _sfxVolume;

    public float GetAmbientVolume() => _ambientVolume;

    #endregion

    #region Private Methods

    private IEnumerator FadeInMusic(AudioClip clip)
    {
        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = 0f;
        _musicSource.Play();
        _isMusicPlaying = true;

        float timer = 0f;
        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(0f, _musicVolume, timer / _fadeDuration);
            yield return null;
        }

        _musicSource.volume = _musicVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = _musicSource.volume;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / _fadeDuration);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.volume = startVolume;
    }

    private IEnumerator FadeOutAmbient()
    {
        float startVolume = _ambientSource.volume;
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            _ambientSource.volume = Mathf.Lerp(startVolume, 0f, timer / _fadeDuration);
            yield return null;
        }

        _ambientSource.Stop();
        _ambientSource.volume = startVolume;
    }

    #endregion
}
